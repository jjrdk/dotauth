namespace DotAuth.Shared.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the client secret.
/// </summary>
public sealed record ClientSecret
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [JsonPropertyName("type")]
    public ClientSecretTypes Type { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    [JsonPropertyName("value")]
    public string Value { get; set; } = null!;
}
