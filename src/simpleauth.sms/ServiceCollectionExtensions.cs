namespace SimpleAuth.Sms
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleAuth.Shared;
    using SimpleAuth.Sms.Services;

    /// <summary>
    /// Defines the service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        ///// <summary>
        ///// Adds the two factor SMS authentication.
        ///// </summary>
        ///// <param name="services">The services.</param>
        ///// <param name="smsOptions">The SMS options.</param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException">
        ///// services
        ///// or
        ///// smsOptions
        ///// </exception>
        //public static IServiceCollection AddTwoFactorSmsAuthentication(
        //    this IServiceCollection services,
        //    TwoFactorSmsOptions smsOptions)
        //{
        //    if (services == null)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }

        //    if (smsOptions == null)
        //    {
        //        throw new ArgumentNullException(nameof(smsOptions));
        //    }

        //    services.AddSingleton(smsOptions);
        //    services.AddTransient<ITwoFactorAuthenticationService, DefaultSmsService>();
        //    return services;
        //}

        /// <summary>
        /// Adds the SMS authentication.
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder.</param>
        /// <param name="smsClient">The SMS client.</param>
        /// <param name="rateLimiter">The rate limiter.</param>
        /// <returns></returns>
        public static IServiceCollection AddSmsAuthentication(this IMvcBuilder mvcBuilder, ISmsClient smsClient, IRateLimiter rateLimiter = null)
        {
            var limiter = rateLimiter ?? new NoopLimiter();
            return AddSmsAuthentication(mvcBuilder, sp => limiter, sp => smsClient);
        }

        /// <summary>
        /// Adds the SMS authentication.
        /// </summary>
        /// <param name="mvcBuilder">The MVC builder.</param>
        /// <param name="rateLimiterFactory">The <see cref="IRateLimiter"/>.</param>
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
            Func<IServiceProvider, IRateLimiter> rateLimiterFactory,
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

            services.AddTransient<Func<IRateLimiter>>(sp => () => rateLimiterFactory(sp));
            services.AddTransient<IRateLimiter>(rateLimiterFactory);
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
