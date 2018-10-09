using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleBus.RabbitMq
{
    public class SimpleBusRabbitMqModule : IModule
    {
        private IDictionary<string, string> _options;

        public void Init(IDictionary<string, string> options)
        {
            _options = options;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleInitialized;
        }

        private void HandleInitialized(object sender, System.EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddSimpleBusRabbitMq(GetOptions());
        }

        private RabbitMqOptions GetOptions()
        {
            var result = new RabbitMqOptions();
            if (_options != null)
            {
                string hostName;
                string brokerName;
                string userName;
                string password;
                int port;
                if(_options.TryGetValue("HostName", out hostName))
                {
                    result.HostName = hostName;
                }

                if (_options.TryGetValue("BrokerName", out brokerName))
                {
                    result.BrokerName = brokerName;
                }

                if (_options.TryGetValue("UserName", out userName))
                {
                    result.UserName = userName;
                }

                if (_options.TryGetValue("Password", out password))
                {
                    result.Password = password;
                }

                if (_options.TryGetValue("Port", out port))
                {
                    result.Port = port;
                }
            }

            return result;
        }
    }
}