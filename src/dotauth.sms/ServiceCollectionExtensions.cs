namespace DotAuth.Sms;

using System;
using DotAuth;
using DotAuth.Endpoints;
using DotAuth.Services;
using DotAuth.Sms.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public static IServiceCollection AddSmsAuthentication(this IMvcCoreBuilder mvcBuilder, ISmsClient smsClient)
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
        this IMvcCoreBuilder mvcBuilder,
        Func<IServiceProvider, ISmsClient> smsClientFactory)
    {
        var assembly = typeof(ISmsClient).Assembly;
        var services = mvcBuilder.Services;
        services.AddAuthentication(CookieNames.PasswordLessCookieName)
            .AddCookie(CookieNames.PasswordLessCookieName, opts => { opts.LoginPath = $"/{SmsConstants.Amr}/Authenticate"; });

        services.AddTransient<Func<ISmsClient>>(sp => () => smsClientFactory(sp));
        services.AddTransient<ISmsClient>(smsClientFactory);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDotAuthUiEndpointRegistration, SmsUiEndpointRegistration>());

        services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
        mvcBuilder.AddApplicationPart(assembly);
        return services;
    }
}
