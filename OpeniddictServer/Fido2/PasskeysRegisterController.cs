using System.Text;
using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OpeniddictServer.Data;

namespace Fido2Identity;

[Route("api/[controller]")]
public class PasskeysRegisterController : Controller
{
    private readonly IFido2 _fido2;
    private readonly Fido2Store _fido2Store;
    private readonly UserManager<ApplicationUser> _userManager;

    private const string PasskeysAttestationOptions = "passkeys.attestationOptions";

    public PasskeysRegisterController(
        IFido2 fido2,
        Fido2Store fido2Store,
        UserManager<ApplicationUser> userManager
        )
    {
        _userManager = userManager;
        _fido2Store = fido2Store;
        _fido2 = fido2;
    }

    private static string FormatException(Exception e)
    {
        return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/makeCredentialOptions")]
    public async Task<JsonResult> MakeCredentialOptions([FromForm] string username, 
                                                        [FromForm] string displayName, 
                                                        [FromForm] string attType, 
                                                        [FromForm] string authType, 
                                                        [FromForm] string residentKey, 
                                                        [FromForm] string userVerification)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
            {
                username = $"{displayName} (Usernameless user created at {DateTime.UtcNow})";
            }

            var applicationUser = await _userManager.FindByEmailAsync(username);
            var user = new Fido2User
            {
                DisplayName = applicationUser.UserName,
                Name = applicationUser.UserName,
                Id = Encoding.UTF8.GetBytes(applicationUser.UserName) // byte representation of userID is required
            };

            // 2. Get user existing keys by username
            var items = await _fido2Store.GetCredentialsByUserNameAsync(applicationUser.UserName);
            var existingKeys = new List<PublicKeyCredentialDescriptor>();
            foreach (var publicKeyCredentialDescriptor in items)
            {
                if (publicKeyCredentialDescriptor.Descriptor != null)
                {
                    existingKeys.Add(publicKeyCredentialDescriptor.Descriptor);
                }
            }

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = residentKey.ToEnum<ResidentKeyRequirement>(),
                UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(authType))
            {
                authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();
            }

            var exts = new AuthenticationExtensionsClientInputs
            { 
                Extensions = true, 
                UserVerificationMethod = true,
                DevicePubKey = new AuthenticationExtensionsDevicePublicKeyInputs() { Attestation = attType },
                CredProps = true
            };

            var options = _fido2.RequestNewCredential(
                user, existingKeys, 
                authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            HttpContext.Session.SetString(PasskeysAttestationOptions, options.ToJson());

            // 5. return options to client
            return Json(options);
        }
        catch (Exception e)
        {
            return Json(new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/makeCredential")]
    public async Task<JsonResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
    {
        try
        {
            // 1. get the options we sent the client
            var jsonOptions = HttpContext.Session.GetString(PasskeysAttestationOptions);
            var options = CredentialCreateOptions.FromJson(jsonOptions);

            // 2. Create callback so that lib can verify credential id is unique to this user
            async Task<bool> callback(IsCredentialIdUniqueToUserParams args, CancellationToken cancellationToken)
            {
                var users = await _fido2Store.GetUsersByCredentialIdAsync(args.CredentialId);
                if (users.Count > 0) return false;

                return true;
            }

            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, callback);

            if(success.Result != null)
            {
                var fidoCredential = new FidoStoredCredential
                {
                    Type = success.Result.Type,
                    CredentialId = success.Result.Id,
                    Descriptor = new PublicKeyCredentialDescriptor(success.Result.Id),
                    PublicKey = success.Result.PublicKey,
                    UserHandle = success.Result.User.Id,
                    UserId = success.Result.User.Id,
                    UserName = success.Result.User.DisplayName,
                    SignCount = success.Result.Counter,
                    CredType = success.Result.CredType,
                    RegDate = DateTime.Now,
                    AaGuid = success.Result.AaGuid,
                    Transports = success.Result.Transports,
                    BE = success.Result.BE,
                    BS = success.Result.BS,
                    AttestationObject = success.Result.AttestationObject,
                    AttestationClientDataJson = success.Result.AttestationClientDataJSON,
                    DevicePublicKeys = new List<byte[]>() { success.Result.DevicePublicKey },
                    IsPasskey = true
                };


                // 3. Store the credentials in db
                await _fido2Store.AddCredentialToUserAsync(options.User, fidoCredential);
            }

            // 4. return "ok" to the client

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new Fido2.CredentialMakeResult("error",  
                        $"Unable to load user with ID '{_userManager.GetUserId(User)}'.",
                        success.Result));
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            //var userId = await _userManager.GetUserIdAsync(user);

            return Json(success);
        }
        catch (Exception e)
        {
            return Json(new Fido2.CredentialMakeResult("error", FormatException(e), null));
        }
    }
}