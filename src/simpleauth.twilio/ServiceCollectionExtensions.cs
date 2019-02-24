namespace SimpleAuth.Twilio
{
    using Controllers;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Services;
    using SimpleAuth.Shared;
    using System;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the two factor SMS authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="twilioOptions">The twilio options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// twilioOptions
        /// </exception>
        public static IServiceCollection AddTwoFactorSmsAuthentication(this IServiceCollection services, TwoFactorTwilioOptions twilioOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (twilioOptions == null)
            {
                throw new ArgumentNullException(nameof(twilioOptions));
            }

            services.AddSingleton(twilioOptions);
            services.AddTransient<ITwoFactorAuthenticationService, DefaultTwilioSmsService>();
            return services;
        }

        /// <summary>
        /// Adds the SMS authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="mvcBuilder">The MVC builder.</param>
        /// <param name="smsAuthenticationOptions">The SMS authentication options.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// mvcBuilder
        /// or
        /// smsAuthenticationOptions
        /// </exception>
        public static IServiceCollection AddSmsAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (smsAuthenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(smsAuthenticationOptions));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedFileProvider);
            });
            services.AddSingleton(smsAuthenticationOptions);
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
