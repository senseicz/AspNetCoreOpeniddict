using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OpeniddictServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginPasskeyModel : PageModel
{
    [BindProperty] public PasskeyModel Passkey { get; set; } = new();

    //[BindProperty(SupportsGet = true)]
    //public string? Email { get; set; }

    //[BindProperty(SupportsGet = true)]
    //public string ReturnUrl { get; set; }

    //[BindProperty(SupportsGet = true)]
    //public bool RememberMe { get; set; }

    public class PasskeyModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }

    public void OnGet(string email, string returnUrl, bool rememberMe)
    {
        Passkey.Email = email;
        Passkey.ReturnUrl = returnUrl;
        Passkey.RememberMe = rememberMe;
    }

    public void OnPost()
    {
    }
}
