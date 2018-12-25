namespace SimpleIdentityServer.Authenticate.SMS
{
    using SimpleIdentityServer.Twilio.Client;
    using SimpleAuth.Server;

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
