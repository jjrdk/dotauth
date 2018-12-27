namespace SimpleAuth.Authenticate.Twilio.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using Server.ViewModels;

    public class SmsOpenIdLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
