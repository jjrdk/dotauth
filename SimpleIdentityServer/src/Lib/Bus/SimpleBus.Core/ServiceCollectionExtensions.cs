using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleBus.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSimpleBus(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IEventPublisher>(new DefaultEventPublisher());
            return services;
        }
    }
}
