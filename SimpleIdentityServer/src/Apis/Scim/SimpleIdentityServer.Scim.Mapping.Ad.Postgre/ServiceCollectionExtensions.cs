using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Postgre
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimMappingPostgreEF(this IServiceCollection serviceCollection, string connectionString, Action<NpgsqlDbContextOptionsBuilder> callback = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            serviceCollection.AddScimMapping();
            serviceCollection.AddEntityFrameworkNpgsql()
                .AddDbContext<MappingDbContext>(options =>
                    options.UseNpgsql(connectionString, callback));
            return serviceCollection;
        }
    }
}
