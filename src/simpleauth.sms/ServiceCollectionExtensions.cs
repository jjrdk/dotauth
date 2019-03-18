namespace SimpleAuth.Sms
{
    using System;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using SimpleAuth.Shared;
    using SimpleAuth.Sms.Controllers;
    using SimpleAuth.Sms.Services;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the two factor SMS authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="smsOptions">The SMS options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// smsOptions
        /// </exception>
        public static IServiceCollection AddTwoFactorSmsAuthentication(
            this IServiceCollection services,
            TwoFactorSmsOptions smsOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (smsOptions == null)
            {
                throw new ArgumentNullException(nameof(smsOptions));
            }

            services.AddSingleton(smsOptions);
            services.AddTransient<ITwoFactorAuthenticationService, DefaultSmsService>();
            return services;
        }

        /// <summary>
        /// Adds the SMS authentication.
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder.</param>
        /// <param name="smsClient">The SMS client.</param>
        /// <returns></returns>
        public static IServiceCollection AddSmsAuthentication(this IMvcBuilder mvcBuilder, ISmsClient smsClient)
        {
            return AddSmsAuthentication(mvcBuilder, sp => smsClient);
        }

        /// <summary>
        /// Adds the SMS authentication.
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder.</param>
        /// <param name="smsClientFactory">The SMS authentication options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// mvcBuilder
        /// or
        /// smsAuthenticationOptions
        /// </exception>
        public static IServiceCollection AddSmsAuthentication(
            this IMvcBuilder mvcBuilder,
            Func<IServiceProvider, ISmsClient> smsClientFactory)
        {
            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            var services = mvcBuilder.Services;
            services.Configure<RazorViewEngineOptions>(opts => { opts.FileProviders.Add(embeddedFileProvider); });
            services.AddTransient<Func<ISmsClient>>(sp => () => smsClientFactory(sp));
            services.AddTransient<ISmsClient>(smsClientFactory);
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
