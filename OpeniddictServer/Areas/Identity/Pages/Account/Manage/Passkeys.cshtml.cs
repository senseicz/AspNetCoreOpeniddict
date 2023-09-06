using Fido2Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpeniddictServer.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class PasskeysModel : PageModel
{
    private readonly Fido2Store _fido2Store;

    public bool HavePasskeys => RegisteredPasskeys != null && RegisteredPasskeys.Any();

    [BindProperty] public ICollection<FidoStoredCredential> RegisteredPasskeys { get; set; }


    public PasskeysModel(Fido2Store fido2Store)
    {
        _fido2Store = fido2Store;
    }


    public async Task OnGet()
    {
        var userName = User.Identity!.Name;
        RegisteredPasskeys = await _fido2Store.GetCredentialsByUserNameAsync(userName);
    }

    public void OnPost()
    {
    }
}
