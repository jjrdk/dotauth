namespace DotAuth.Shared.Responses;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the profile response.
/// </summary>
public sealed record ProfileResponse
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    /// <value>
    /// The issuer.
    /// </value>
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the create date time.
    /// </summary>
    /// <value>
    /// The create date time.
    /// </value>
    [JsonPropertyName("create_datetime")]
    public DateTimeOffset CreateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the update time.
    /// </summary>
    /// <value>
    /// The update time.
    /// </value>
    [JsonPropertyName("update_datetime")]
    public DateTimeOffset UpdateTime { get; set; }
}
