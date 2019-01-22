namespace SimpleAuth.Twilio.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class SmsAuthenticationViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
