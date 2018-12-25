namespace SimpleIdentityServer.Authenticate.LoginPassword.ViewModels
{
    using SimpleAuth.Server.ViewModels;

    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
