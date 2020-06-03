namespace SimpleAuth.Sms
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Sms.Services;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
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

            var assembly = typeof(ISmsClient).Assembly;
            var services = mvcBuilder.Services;
            services.AddAuthentication(CookieNames.PasswordLessCookieName)
                .AddCookie(CookieNames.PasswordLessCookieName, opts => { opts.LoginPath = $"/{SmsConstants.Amr}/Authenticate"; });

            services.AddTransient<Func<ISmsClient>>(sp => () => smsClientFactory(sp));
            services.AddTransient<ISmsClient>(smsClientFactory);

            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
