namespace OpeniddictServer.Models
{
    public record LoginModel(string Email, bool RememberMe, string ReturnUrl);
    public record PasswordModel(string Email, bool RememberMe, string ReturnUrl);
    public record MfaModel(string Email, bool RememberMe, string ReturnUrl);



    //public class LoginModel : BasePassthruModel
    //{
    //    public LoginModel(string email, bool rememberMe, string returnUrl) : base(email, rememberMe, returnUrl)
    //    {
    //    }
    //}

    //public class PasswordModel : BasePassthruModel
    //{
    //    public PasswordModel(string email, bool rememberMe, string returnUrl) : base(email, rememberMe, returnUrl)
    //    {
    //    }
    //}

    //public class MfaModel : BasePassthruModel
    //{
    //    public MfaModel(string email, bool rememberMe, string returnUrl) : base(email, rememberMe, returnUrl)
    //    {
    //    }
    //}

    //public class BasePassthruModel
    //{
    //    public BasePassthruModel()
    //    {
    //    }

    //    public BasePassthruModel(string email, bool rememberMe, string returnUrl)
    //    {
    //        Email = email;
    //        RememberMe = rememberMe;
    //        ReturnUrl = returnUrl;
    //    }

    //    public string Email { get; init; }
    //    public bool RememberMe { get; init; }
    //    public string ReturnUrl { get; init; }

    //}


    public static class ModelNames
    {
        public const string LoginModel = "loginModel";
        public const string PasswordModel = "passwordModel";
        public const string MfaModel = "mfaModel";
    }

}
