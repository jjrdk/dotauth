namespace SimpleAuth.AcceptanceTests
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Client;

    /// <summary>
    /// Defines extensions to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds UMA dependencies to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add dependencies to.</param>
        /// <param name="umaAuthority">The <see cref="Uri"/> where to find the discovery document.</param>
        /// <returns></returns>
        public static IServiceCollection AddUmaClient(this IServiceCollection serviceCollection, Uri umaAuthority)
        {
            serviceCollection.AddSingleton(sp => new UmaClient(sp.GetRequiredService<HttpClient>(), umaAuthority));
            serviceCollection.AddTransient<IUmaPermissionClient, UmaClient>(sp => sp.GetRequiredService<UmaClient>());

            return serviceCollection;
        }
    }
}