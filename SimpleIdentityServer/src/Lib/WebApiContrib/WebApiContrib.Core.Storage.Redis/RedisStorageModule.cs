using Microsoft.Extensions.Caching.Redis;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace WebApiContrib.Core.Storage.Redis
{
    public class RedisStorageModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }

        private void HandleServiceContextInitialized(object sender, EventArgs e)
        {
            var kvp = GetOptions();
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddStorage(u => u.UseRedis(kvp.Key, kvp.Value));
        }

        private KeyValuePair<RedisCacheOptions, int> GetOptions()
        {
            int port = 6379;
            var redisCacheOptions = new RedisCacheOptions();
            if (_properties != null)
            {
                var configuration = string.Empty;
                var instanceName = string.Empty;
                if (_properties.TryGetValue("Configuration", out configuration))
                {
                    redisCacheOptions.Configuration = configuration;
                }

                if (_properties.TryGetValue("InstanceName", out instanceName))
                {
                    redisCacheOptions.InstanceName = instanceName;
                }

                _properties.TryGetValue("Port", out port);
            }

            return new KeyValuePair<RedisCacheOptions, int>(redisCacheOptions, port);
        }
    }
}
