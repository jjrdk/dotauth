namespace DotAuth.Shared.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the subject response.
/// </summary>
public sealed record SubjectResponse
{
    /// <summary>
    /// Gets or sets the subject identifier.
    /// </summary>
    /// <value>
    /// The subject identifier.
    /// </value>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = null!;
}
