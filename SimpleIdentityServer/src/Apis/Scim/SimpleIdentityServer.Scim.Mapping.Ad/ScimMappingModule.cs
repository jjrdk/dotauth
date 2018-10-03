using Newtonsoft.Json;
using SimpleIdentityServer.Module;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    public class ScimMappingModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddScimMapping(adConfiguration: GetConfiguration());
        }

        private AdConfiguration GetConfiguration()
        {
            var result = new AdConfiguration();
            if (_properties != null)
            {
                string ipAdr;
                int port;
                string userName;
                string password;
                string distinguishedName;
                bool isEnabled;
                string adConfigurationSchema;
                if (_properties.TryGetValue("IpAdr", out ipAdr))
                {
                    result.IpAdr = ipAdr;
                }

                if (_properties.TryGetValue("Port", out port))
                {
                    result.Port = port;
                }

                if(_properties.TryGetValue("UserName", out userName))
                {
                    result.Username = userName;
                }

                if (_properties.TryGetValue("Password", out password))
                {
                    result.Password = password;
                }

                if (_properties.TryGetValue("DistinguishedName", out distinguishedName))
                {
                    result.DistinguishedName = distinguishedName;
                }

                if (_properties.TryGetValue("IsEnabled", out isEnabled))
                {
                    result.IsEnabled = isEnabled;
                }

                if (_properties.TryGetValue("AdConfigurationSchemas", out adConfigurationSchema))
                {
                    result.AdConfigurationSchemas = JsonConvert.DeserializeObject<List<AdConfigurationSchema>>(adConfigurationSchema);
                }
            }

            return result;
        }
    }
}
