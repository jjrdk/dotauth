namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the link profile request.
/// </summary>
public sealed record LinkProfileRequest
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
    /// Gets or sets a value indicating whether this <see cref="LinkProfileRequest"/> is force.
    /// </summary>
    /// <value>
    ///   <c>true</c> if force; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("force")]
    public bool Force { get; set; }
}
