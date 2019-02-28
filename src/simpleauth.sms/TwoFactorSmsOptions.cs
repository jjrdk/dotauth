namespace SimpleAuth.Sms
{
    /// <summary>
    /// Defines the two factor Twilio options.
    /// </summary>
    public class TwoFactorSmsOptions
    {
        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        /// <value>
        /// The SMS message.
        /// </value>
        public string SmsMessage { get; set; }
    }
}