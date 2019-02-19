namespace SimpleAuth.Twilio
{
    /// <summary>
    /// Defines the two factor Twilio options.
    /// </summary>
    public class TwoFactorTwilioOptions
    {
        /// <summary>
        /// Gets or sets the twilio account sid.
        /// </summary>
        /// <value>
        /// The twilio account sid.
        /// </value>
        public string TwilioAccountSid { get; set; }

        /// <summary>
        /// Gets or sets the twilio authentication token.
        /// </summary>
        /// <value>
        /// The twilio authentication token.
        /// </value>
        public string TwilioAuthToken { get; set; }

        /// <summary>
        /// Gets or sets the twilio from number.
        /// </summary>
        /// <value>
        /// The twilio from number.
        /// </value>
        public string TwilioFromNumber { get; set; }

        /// <summary>
        /// Gets or sets the twilio message.
        /// </summary>
        /// <value>
        /// The twilio message.
        /// </value>
        public string TwilioMessage { get; set; }
    }
}