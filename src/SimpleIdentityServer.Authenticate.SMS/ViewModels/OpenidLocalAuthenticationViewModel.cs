namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using Host.ViewModels;

    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
