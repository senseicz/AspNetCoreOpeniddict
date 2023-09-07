// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpeniddictServer.Data;
using Fido2Identity;
using OpeniddictServer.Helpers;
using OpeniddictServer.Models;

namespace OpeniddictServer.Areas.Identity.Pages.Account
{
    public class PasswordModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Fido2Store _fido2Store;
        private readonly ILogger<PasswordModel> _logger;

        public PasswordModel(SignInManager<ApplicationUser> signInManager,
            Fido2Store fido2Store,
            ILogger<PasswordModel> logger)
        {
            _signInManager = signInManager;
            _fido2Store = fido2Store;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string Email { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public IActionResult OnGet()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            var pwdModel = TempData.Get<Models.PasswordModel>(ModelNames.PasswordModel);

            if (pwdModel != null)
            {
                Email = pwdModel.Email;
                ReturnUrl = pwdModel.ReturnUrl;
                RememberMe = pwdModel.RememberMe;
            }
            else
            {
                //go back to Login page
                return RedirectToPage("./Login");
            }

            TempData.Put(ModelNames.PasswordModel, pwdModel);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var pwdModel = TempData.Get<Models.PasswordModel>(ModelNames.PasswordModel);
                if (pwdModel != null)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(pwdModel.Email, Input.Password, pwdModel.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(pwdModel.ReturnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
