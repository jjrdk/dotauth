namespace DotAuth.Shared.Responses;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the update policy response.
/// </summary>
public sealed record PolicyRuleResponse
{
    /// <summary>
    /// Gets or sets the client ids allowed.
    /// </summary>
    /// <value>
    /// The client ids allowed.
    /// </value>
    [JsonPropertyName("clients")]
    public string[] ClientIdsAllowed { get; set; } = [];

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [JsonPropertyName("claims")]
    public ClaimData[] Claims { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this instance is resource owner consent needed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("consent_needed")]
    public bool IsResourceOwnerConsentNeeded { get; set; }

    /// <summary>
    /// Gets or sets the script.
    /// </summary>
    /// <value>
    /// The script.
    /// </value>
    [JsonPropertyName("script")]
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the open identifier provider.
    /// </summary>
    /// <value>
    /// The open identifier provider.
    /// </value>
    [JsonPropertyName("provider")]
    public string? OpenIdProvider { get; set; }
}
