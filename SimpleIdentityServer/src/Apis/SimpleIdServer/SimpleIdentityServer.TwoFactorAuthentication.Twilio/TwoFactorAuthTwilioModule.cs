using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.TwoFactorAuthentication.Twilio
{
    public class TwoFactorAuthTwilioModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }
        
        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddTwoFactorSmsAuthentication(GetOptions());
        }

        private TwoFactorTwilioOptions GetOptions()
        {
            var result = new TwoFactorTwilioOptions();
            if (_properties != null)
            {
                var accountSid = string.Empty;
                var authToken = string.Empty;
                var fromNumber = string.Empty;
                var message = string.Empty;
                if (_properties.TryGetValue("AccountSid", out accountSid))
                {
                    result.TwilioAccountSid = accountSid;
                }

                if (_properties.TryGetValue("AuthToken", out authToken))
                {
                    result.TwilioAuthToken = authToken;
                }

                if (_properties.TryGetValue("FromNumber", out fromNumber))
                {
                    result.TwilioFromNumber = fromNumber;
                }

                if (_properties.TryGetValue("Message", out message))
                {
                    result.TwilioMessage = message;
                }
            }

            return result;
        }
    }
}