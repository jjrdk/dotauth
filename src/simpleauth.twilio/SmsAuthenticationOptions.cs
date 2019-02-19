namespace SimpleAuth.Twilio
{
    /// <summary>
    /// Defines the SMS authentication options.
    /// </summary>
    public class SmsAuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsAuthenticationOptions"/> class.
        /// </summary>
        public SmsAuthenticationOptions()
        {
            Message = "The confirmation code is {0}";
            TwilioSmsCredentials = new TwilioSmsCredentials();
        }

        /// <summary>
        /// Gets or sets the twilio SMS credentials.
        /// </summary>
        /// <value>
        /// The twilio SMS credentials.
        /// </value>
        public TwilioSmsCredentials TwilioSmsCredentials { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
    }
}
