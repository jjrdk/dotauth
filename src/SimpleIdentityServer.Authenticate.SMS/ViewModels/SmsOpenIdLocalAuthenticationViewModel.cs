namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using SimpleAuth.Server.ViewModels;

    public class SmsOpenIdLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
