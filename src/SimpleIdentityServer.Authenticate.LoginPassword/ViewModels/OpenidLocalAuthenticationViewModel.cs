namespace SimpleIdentityServer.Authenticate.LoginPassword.ViewModels
{
    using Host.ViewModels;

    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
