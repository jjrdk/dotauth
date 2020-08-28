namespace SimpleAuth.Sms.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the SMS authentication view model.
    /// </summary>
    public class SmsAuthenticationViewModel
    {
        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [Required]
        public string? PhoneNumber { get; set; }
    }
}
