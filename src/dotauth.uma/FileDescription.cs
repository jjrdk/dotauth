namespace DotAuth.Uma;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the slim resource.
/// </summary>
public class FileDescription
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = null!;

    /// <summary>
    /// Gets or sets the format of the content.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the resource mime type.
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the byte size of the resource.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }
}
