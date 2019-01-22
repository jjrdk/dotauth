namespace SimpleAuth.Twilio
{
    public class TwilioSmsCredentials
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; }
    }
}