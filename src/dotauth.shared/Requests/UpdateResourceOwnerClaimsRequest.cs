namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the request to update resource owner claims.
/// </summary>
public sealed record UpdateResourceOwnerClaimsRequest
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The login.
    /// </value>
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [JsonPropertyName("claims")]
    public ClaimData[] Claims { get; set; } = [];
}
