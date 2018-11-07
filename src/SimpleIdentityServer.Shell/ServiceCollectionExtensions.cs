using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Shell.Controllers;
using System;

namespace SimpleIdentityServer.Shell
{
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.FileProviders;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBasicShell(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            var assembly = typeof(HomeController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });

            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
