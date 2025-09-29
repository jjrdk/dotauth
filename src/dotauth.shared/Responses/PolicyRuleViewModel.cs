namespace DotAuth.Shared.Responses;

using System.Text.Json.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the policy rule view model.
/// </summary>
public sealed record PolicyRuleViewModel
{
    /// <summary>
    /// Gets or sets a comma separated string with allowed client ids.
    /// </summary>
    [JsonPropertyName("client_ids_allowed")]
    public string? ClientIdsAllowed { get; set; }

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    [JsonPropertyName("scopes")]
    public string? Scopes { get; set; }

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    [JsonPropertyName("claims")]
    public ClaimData[] Claims { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this instance is resource owner consent needed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is resource owner consent needed; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("is_resource_owner_consent_needed")]
    public bool IsResourceOwnerConsentNeeded { get; set; }

    /// <summary>
    /// Gets or sets the open identifier provider.
    /// </summary>
    [JsonPropertyName("openid_provider")]
    public string? OpenIdProvider { get; set; }
}
