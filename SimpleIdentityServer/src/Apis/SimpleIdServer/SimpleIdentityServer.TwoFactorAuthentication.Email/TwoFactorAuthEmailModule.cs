using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.TwoFactorAuthentication.Email
{
    public class TwoFactorAuthEmailModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddEmailTwoFactorAuthentication(GetOptions());
        }

        private TwoFactorEmailOptions GetOptions()
        {
            var result = new TwoFactorEmailOptions();
            if(_properties != null)
            {
                string emailFromName;
                string emailFromAddress;
                string emailSubject;
                string emailBody;
                string emailSmtpHost;
                int emailSmtpPort;
                string authenticationType;
                string emailUserName;
                string emailPassword;
                if (_properties.TryGetValue("EmailFromName", out emailFromName))
                {
                    result.EmailFromName = emailFromName;
                }

                if (_properties.TryGetValue("EmailFromAddress", out emailFromAddress))
                {
                    result.EmailFromAddress = emailFromAddress;
                }

                if (_properties.TryGetValue("EmailSubject", out emailSubject))
                {
                    result.EmailSubject = emailSubject;
                }

                if (_properties.TryGetValue("EmailBody", out emailBody))
                {
                    result.EmailBody = emailBody;
                }

                if (_properties.TryGetValue("EmailSmtpHost", out emailSmtpHost))
                {
                    result.EmailSmtpHost = emailSmtpHost;
                }

                if (_properties.TryGetValue("EmailSmtpPort", out emailSmtpPort))
                {
                    result.EmailSmtpPort = emailSmtpPort;
                }

                if (_properties.TryGetValue("AuthenticationType", out authenticationType))
                {
                    if (authenticationType == "ssl")
                    {
                        result.AuthenticationType = AuthenticationTypes.SSL;
                    }
                    else if (authenticationType == "tls")
                    {
                        result.AuthenticationType = AuthenticationTypes.TLS;    
                    }
                    else
                    {
                        result.AuthenticationType = AuthenticationTypes.None;
                    }
                }

                if (_properties.TryGetValue("EmailUserName", out emailUserName))
                {
                    result.EmailUserName = emailUserName;
                }

                if (_properties.TryGetValue("EmailPassword", out emailPassword))
                {
                    result.EmailPassword = emailPassword;
                }
            }

            return result;
        }
    }
}