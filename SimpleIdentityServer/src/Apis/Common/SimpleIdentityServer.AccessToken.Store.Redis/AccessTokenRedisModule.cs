using Microsoft.Extensions.Caching.Redis;
using SimpleIdentityServer.Module;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccessToken.Store.Redis
{
    public class AccessTokenRedisModule : IModule
    {
        private class AccessTokenOptions
        {
            public RedisCacheOptions RedisCacheOptions { get; set; }
            public int Port { get; set; }
            public int SlidingExpirationTime { get; set; }
        }

        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleInitialized;
        }

        private void HandleInitialized(object sender, EventArgs eventArgs)
        {
            var o = GetOptions();
            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddRedisTokenStore(o.RedisCacheOptions, o.Port, o.SlidingExpirationTime);
        }

        private AccessTokenOptions GetOptions()
        {
            int port = 6379;
            int slidingExpirationTime = 3600;
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
                _properties.TryGetValue("SlidingExpirationTime", out slidingExpirationTime);
            }

            return new AccessTokenOptions
            {
                Port = port,
                RedisCacheOptions = redisCacheOptions,
                SlidingExpirationTime = slidingExpirationTime
            };
        }
    }
}