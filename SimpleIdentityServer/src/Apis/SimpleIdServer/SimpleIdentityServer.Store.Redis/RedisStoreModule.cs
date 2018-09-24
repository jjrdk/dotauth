using Microsoft.Extensions.Caching.Redis;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Store.Redis
{
    public class RedisStoreModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleInitialized;
        }

        private void HandleInitialized(object sender, EventArgs e)
        {
            var options = GetOptions();
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddRedisStorage(options.Key, options.Value);
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