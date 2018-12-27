namespace SimpleAuth.Authenticate.Twilio
{
    public class TwoFactorTwilioOptions
    {
        public string TwilioAccountSid { get; set; }
        public string TwilioAuthToken { get; set; }
        public string TwilioFromNumber { get; set; }
        public string TwilioMessage { get; set; }
    }
}