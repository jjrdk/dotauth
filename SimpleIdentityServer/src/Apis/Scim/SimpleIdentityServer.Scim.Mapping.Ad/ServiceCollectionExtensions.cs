using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Mapping.Ad.Models;
using SimpleIdentityServer.Scim.Mapping.Ad.Stores;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimMapping(this IServiceCollection services, List<AdMapping> adMappings = null, AdConfiguration adConfiguration = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IAttributeMapper, AttributeMapper>();
            services.AddTransient<IUserFilterParser, UserFilterParser>();
            services.AddSingleton<IMappingStore>(new DefaultMappingStore(adMappings));
            services.AddSingleton<IConfigurationStore>(new DefaultConfigurationStore(adConfiguration));
            return services;
        }

        public static IServiceCollection AddMappingStoreEF(this IServiceCollection services)
        {
            services.AddTransient<IMappingStore, MappingStore>();
            return services;
        }
    }
}