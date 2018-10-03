using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApiContrib.Core.Storage.Redis
{
    public static class StorageOptionsBuilderExtensions
    {
        public static void UseRedis(this StorageOptionsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            var storage = new RedisStorage();
            builder.StorageOptions.Storage = storage;
            builder.ServiceCollection.AddSingleton<IStorage>(storage);
        }
    }
}
