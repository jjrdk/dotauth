using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimMappingSqliteEF(this IServiceCollection serviceCollection, string connectionString, Action<SqliteDbContextOptionsBuilder> callback = null)
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
            serviceCollection.AddEntityFrameworkSqlite()
                .AddDbContext<MappingDbContext>(options =>
                    options.UseSqlite(connectionString, callback));
            return serviceCollection;
        }
    }
}
