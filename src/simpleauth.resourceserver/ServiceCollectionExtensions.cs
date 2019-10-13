namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Client;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUma(this IServiceCollection serviceCollection, Uri configurationUri)
        {
            serviceCollection.AddSingleton(
                sp => new UmaClient(sp.GetService<HttpClient>(), configurationUri));
            serviceCollection.AddTransient<IProvideUmaConfiguration, UmaClient>(sp => sp.GetService<UmaClient>());

            return serviceCollection;
        }
    }
}