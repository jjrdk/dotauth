using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Client;
using System;

namespace SimpleIdentityServer.AccessToken.Store
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultAccessTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IAccessTokenStore>(new InMemoryTokenStore(new IdentityServerClientFactory()));
            return serviceCollection;
        }
    }
}
