using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimMappingSqlServerEF(this IServiceCollection serviceCollection, string connectionString, Action<SqlServerDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkSqlServer()
                .AddDbContext<MappingDbContext>(options =>
                    options.UseSqlServer(connectionString, callback));
            return serviceCollection;
        }
    }
}
