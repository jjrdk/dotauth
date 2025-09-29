namespace DotAuth.Shared.Requests;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the add resource owner request.
/// </summary>
public sealed record AddResourceOwnerRequest
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The subject.
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
