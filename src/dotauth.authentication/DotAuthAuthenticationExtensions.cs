namespace dotauth.authentication;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Defines the extensions to <see cref="AuthenticationBuilder"/>.
/// </summary>
public static class DotAuthAuthenticationExtensions
{
    /// <summary>
    /// Adds OAuth 2.0 based authentication to <see cref="AuthenticationBuilder"/> using the specified authentication scheme.
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="displayName">A display name for the authentication handler.</param>
    /// <param name="configureOptions">A delegate to configure the handler specific options.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddDotAuth<
            TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        string displayName,
        Action<TOptions> configureOptions)
        where TOptions : DotAuthOptions, new()
        where THandler : DotAuthHandler<TOptions>
    {
        return builder.AddRemoteScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
    }

    /// <summary>
    /// Adds OAuth 2.0 based authentication to <see cref="AuthenticationBuilder"/> using the specified authentication scheme.
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="configureOptions">A delegate to configure the handler specific options.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddDotAuth<
            TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this AuthenticationBuilder builder,
        Action<TOptions> configureOptions)
        where TOptions : DotAuthOptions, new()
        where THandler : DotAuthHandler<TOptions>
    {
        return builder.AddRemoteScheme<TOptions, THandler>(DotAuthDefaults.AuthenticationScheme,
            DotAuthDefaults.AuthenticationScheme, configureOptions);
    }

    /// <summary>
    /// Adds OAuth 2.0 based authentication to <see cref="AuthenticationBuilder"/> using the specified authentication scheme.
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddDotAuth<
            TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this AuthenticationBuilder builder)
        where TOptions : DotAuthOptions, new()
        where THandler : DotAuthHandler<TOptions>
    {
        return builder.AddRemoteScheme<TOptions, THandler>(DotAuthDefaults.AuthenticationScheme,
            DotAuthDefaults.AuthenticationScheme, _ => { });
    }
}
