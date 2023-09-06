using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OpeniddictServer.Data;

namespace Fido2Identity;

[Route("api/[controller]")]
public class PasskeysSignInController : Controller
{
    private readonly IFido2 _fido2;
    private readonly Fido2Store _fido2Store;
    private readonly SignInManager<ApplicationUser> _signInManager;

    private const string PasskeysAssertionOptions = "passkeys.assertionOptions";

    public PasskeysSignInController(
        IFido2 fido2,
        Fido2Store fido2Store,
        SignInManager<ApplicationUser> signInManager)
    {
        _fido2 = fido2;
        _signInManager = signInManager;
        _fido2Store = fido2Store;

    }

    private static string FormatException(Exception e)
    {
        return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/assertionOptions")]
    public async Task<ActionResult> AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username is empty");
            }

            var applicationUser = await _signInManager.UserManager.FindByEmailAsync(username);

            if (applicationUser == null)
            {
                throw new ArgumentException("Username was not registered");
            }

            var existingCredentials = new List<PublicKeyCredentialDescriptor>();

            var storedCredentials = await _fido2Store.GetCredentialsByUserNameAsync(username);

            if (storedCredentials == null || !storedCredentials.Any())
            {
                throw new ArgumentException("No passkey registered for " + username);
            }

            existingCredentials = storedCredentials.Select(c => c.Descriptor).NotNull().ToList();

            var exts = new AuthenticationExtensionsClientInputs()
            {
                Extensions = true,
                UserVerificationMethod = true,
                DevicePubKey = new AuthenticationExtensionsDevicePublicKeyInputs()
            };

            // 3. Create options
            var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
            var options = _fido2.GetAssertionOptions(
                existingCredentials,
                uv,
                exts
            );

            // 4. Temporarily store options, session/in-memory cache/redis/db
            HttpContext.Session.SetString(PasskeysAssertionOptions, options.ToJson());

            // 5. Return options to client
            return Json(options);
        }

        catch (Exception e)
        {
            return Json(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/makeAssertion")]
    public async Task<JsonResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse, [FromHeader] bool rememberMe)
    {
        try
        {
            // 1. Get the assertion options we sent the client
            var jsonOptions = HttpContext.Session.GetString(PasskeysAssertionOptions);
            var options = AssertionOptions.FromJson(jsonOptions);

            // 2. Get registered credential from database
            var creds = await _fido2Store.GetCredentialByIdAsync(clientResponse.Id);

            if (creds == null)
            {
                throw new Exception("Unknown credentials");
            }

            // 3. Get credential counter from database
            var storedCounter = creds.SignCount;

            // 4. Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                var storedCreds = await _fido2Store.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.Any(c => c.Descriptor != null && c.Descriptor.Id.SequenceEqual(args.CredentialId));
            };

            if (creds.PublicKey == null)
            {
                throw new InvalidOperationException($"No public key");
            }

            // 5. Make the assertion
            var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, creds.DevicePublicKeys, storedCounter, callback);

            // 6. Store the updated counter
            await _fido2Store.UpdateCounterAsync(res.CredentialId, res.Counter);

            // complete sign-in
            var userName = creds.UserName!;

            var user = await _signInManager.UserManager.FindByEmailAsync(userName);
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load user {userName}.");
            }

            await _signInManager.SignInAsync(user, rememberMe);

            // 7. return OK to client
            return Json(res);
        }
        catch (Exception e)
        {
            return Json(new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) });
        }
    }
}
