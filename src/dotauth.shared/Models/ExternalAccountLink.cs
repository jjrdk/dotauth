namespace DotAuth.Shared.Models;

using System;
using System.Security.Claims;

/// <summary>
/// Defines the external link content.
/// </summary>
public sealed record ExternalAccountLink
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    /// <value>
    /// The issuer.
    /// </value>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the external claims.
    /// </summary>
    /// <value>
    /// The external claims.
    /// </value>
    public Claim[] ExternalClaims { get; set; } = Array.Empty<Claim>();
}