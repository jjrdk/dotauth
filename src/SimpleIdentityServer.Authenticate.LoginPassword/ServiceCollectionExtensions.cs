using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Authenticate.LoginPassword.Controllers;
using SimpleIdentityServer.Authenticate.LoginPassword.Services;
using System;
using System.Reflection;

namespace SimpleIdentityServer.Authenticate.LoginPassword
{
    using Host;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.CodeAnalysis;
    using Microsoft.Extensions.FileProviders;
    using SimpleAuth.Services;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoginPasswordAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, BasicAuthenticateOptions basicAuthenticateOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (basicAuthenticateOptions == null)
            {
                throw new ArgumentNullException(nameof(basicAuthenticateOptions));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedFileProvider);
                opts.CompilationCallback = (context) =>
                {
                    var asm = MetadataReference.CreateFromFile(Assembly.Load(typeof(HostConstants).Assembly.GetName()).Location);
                    context.Compilation = context.Compilation.AddReferences(asm);
                };
            });
            services.AddSingleton(basicAuthenticateOptions);
            services.AddTransient<IAuthenticateResourceOwnerService, PasswordAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
