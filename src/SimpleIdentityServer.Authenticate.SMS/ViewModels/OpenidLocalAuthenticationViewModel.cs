using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    using Host.ViewModels;

    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
