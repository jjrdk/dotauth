namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the request to update a resource owner password.
/// </summary>
public sealed record UpdateResourceOwnerPasswordRequest
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
    /// Gets or sets the password.
    /// </summary>
    /// <value>
    /// The password.
    /// </value>
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
