// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpeniddictServer.Data;
using Fido2Identity;
using OpeniddictServer.Helpers;
using OpeniddictServer.Models;

namespace OpeniddictServer.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Fido2Store _fido2Store;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            Fido2Store fido2Store,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _fido2Store = fido2Store;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public bool RenderPageForPasskeyChallenge { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                //ensure we know this user
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                //find out whether user have Passkey/2FA registered
                if (user.TwoFactorEnabled)
                {
                    var fido2ItemExistsForUser = await _fido2Store.GetCredentialsByUserNameAsync(Input.Email);

                    //prefer Passkeys login over any other stored credentials
                    if (fido2ItemExistsForUser.Any(c => c.IsPasskey))
                    {
                        RenderPageForPasskeyChallenge = true;
                        return Page();
                    }

                    if (fido2ItemExistsForUser.Count > 0)
                    {
                        return RedirectToPage("./LoginFido2Mfa", new { ReturnUrl = returnUrl, Input.RememberMe });
                    }

                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                else
                {
                    var passwordModel = new Models.PasswordModel(Input.Email, Input.RememberMe, returnUrl);
                    TempData.Put(ModelNames.PasswordModel,passwordModel);
                    return RedirectToPage("./Password");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
