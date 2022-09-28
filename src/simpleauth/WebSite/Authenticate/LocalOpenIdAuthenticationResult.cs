namespace SimpleAuth.WebSite.Authenticate;

using System;
using System.Security.Claims;
using SimpleAuth.Results;

/// <summary>
/// Defines the local OpenId authentication resultKind.
/// </summary>
internal sealed class LocalOpenIdAuthenticationResult
{
    /// <summary>
    /// Gets or sets the endpoint resultKind.
    /// </summary>
    /// <value>
    /// The endpoint resultKind.
    /// </value>
    public EndpointResult? EndpointResult { get; set; }

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    public Claim[] Claims { get; set; } = Array.Empty<Claim>();

    /// <summary>
    /// Gets or sets the two factor.
    /// </summary>
    /// <value>
    /// The two factor.
    /// </value>
    public string? TwoFactor { get; set; }

    /// <summary>
    /// Gets or sets an error message for the authentication.
    /// </summary>
    public string? ErrorMessage { get; set; }
}