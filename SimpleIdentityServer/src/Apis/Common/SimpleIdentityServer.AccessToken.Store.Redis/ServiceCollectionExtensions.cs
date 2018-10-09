using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.AccessToken.Store.Redis
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisTokenStore(this IServiceCollection services, RedisCacheOptions options, int port = 6379, int slidingExpirationTime = 3600)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!IsIpAddress(options.Configuration))
            {
                IPHostEntry ip = Dns.GetHostEntryAsync(options.Configuration).Result;
                var ipAddress = string.Empty;
                foreach (var adr in ip.AddressList)
                {
                    var strIp = adr.ToString();
                    if (IsIpAddress(strIp))
                    {
                        ipAddress = strIp;
                        break;
                    }
                }

                options.Configuration = ipAddress;
            }

            var redisStorage = new RedisStorage(options, port);
            var identityServerClientFactory = new IdentityServerClientFactory();
            services.AddSingleton<IAccessTokenStore>(new RedisTokenStore(redisStorage, identityServerClientFactory, slidingExpirationTime));
            return services;
        }

        private static bool IsIpAddress(string host)
        {
            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            return Regex.IsMatch(host, ipPattern);
        }
    }
}
