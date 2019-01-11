namespace SimpleAuth.Twilio.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using SimpleAuth.ViewModels;

    public class SmsOpenIdLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
