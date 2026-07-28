namespace DotAuth.Uma.Web;

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods to configure Uma bearer authentication.
/// </summary>
public static class UmaBearerExtensions
{
    /// <summary>
    /// Enables Uma-bearer authentication using the default scheme <see cref="AuthenticationScheme"/>.
    /// <para>
    /// Uma bearer authentication performs authentication by extracting and validating a Uma token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddUmaBearer(this AuthenticationBuilder builder)
        => builder.AddUmaBearer(UmaBearerDefaults.AuthenticationScheme, _ => { });

    /// <summary>
    /// Enables Uma-bearer authentication using a pre-defined scheme.
    /// <para>
    /// Uma bearer authentication performs authentication by extracting and validating a Uma token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddUmaBearer(this AuthenticationBuilder builder, string authenticationScheme)
        => builder.AddUmaBearer(authenticationScheme, _ => { });

    /// <summary>
    /// Enables Uma-bearer authentication using the default scheme <see cref="AuthenticationScheme"/>.
    /// <para>
    /// Uma bearer authentication performs authentication by extracting and validating a Uma token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="UmaBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddUmaBearer(this AuthenticationBuilder builder, Action<UmaBearerOptions> configureOptions)
        => builder.AddUmaBearer(UmaBearerDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Enables Uma-bearer authentication using the specified scheme.
    /// <para>
    /// Uma bearer authentication performs authentication by extracting and validating a Uma token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="UmaBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddUmaBearer(this AuthenticationBuilder builder, string authenticationScheme, Action<UmaBearerOptions> configureOptions)
        => builder.AddUmaBearer(authenticationScheme, displayName: null, configureOptions: configureOptions);

    /// <summary>
    /// Enables Uma-bearer authentication using the specified scheme.
    /// <para>
    /// Uma bearer authentication performs authentication by extracting and validating a Uma token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="displayName">The display name for the authentication handler.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="UmaBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddUmaBearer(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<UmaBearerOptions> configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<UmaBearerOptions>, UmaBearerConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<UmaBearerOptions>, UmaBearerPostConfigureOptions>());
        return builder.AddScheme<UmaBearerOptions, UmaBearerHandler>(authenticationScheme, displayName, configureOptions);
    }
}
