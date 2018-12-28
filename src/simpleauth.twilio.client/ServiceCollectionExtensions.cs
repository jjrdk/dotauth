namespace SimpleAuth.Twilio.Client
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthenticateSmsClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //services.AddTransient<IHttpClientFactory, HttpClientFactory>();
            services.AddTransient<ISidSmsAuthenticateClient, SidSmsAuthenticateClient>();
            //services.AddTransient<ISendSmsOperation, SendSmsOperation>();
            return services;
        }
    }
}
