namespace DotAuth.Shared.Responses;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the view model for editing policies.
/// </summary>
public sealed record EditPolicyResponse
{
    /// <summary>
    /// Gets or sets the resource id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authorization policies.
    /// </summary>
    [JsonPropertyName("rules")]
    public PolicyRuleViewModel[] Rules { get; set; } = [];
}
