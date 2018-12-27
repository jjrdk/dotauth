namespace SimpleAuth.Authenticate.Twilio
{
    using Server;
    using SimpleIdentityServer.Twilio.Client;

    public class SmsAuthenticationOptions : BasicAuthenticateOptions
    {
        public SmsAuthenticationOptions()
        {
            Message = "The confirmation code is {0}";
            TwilioSmsCredentials = new TwilioSmsCredentials();
        }

        public TwilioSmsCredentials TwilioSmsCredentials { get; set; }
        public string Message { get; set; }
    }
}
