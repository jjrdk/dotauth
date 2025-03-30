namespace DotAuth.Shared.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the add resource owner response.
/// </summary>
public sealed class AddResourceOwnerResponse
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }
}
