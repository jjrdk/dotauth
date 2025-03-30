namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the resource registration.
/// </summary>
public class FileDetails : FileDescription
{
    /// <summary>
    /// Gets or sets the resource file name
    /// </summary>
    [JsonPropertyName("location")]
    public Uri Location { get; set; } = null!;

    public FileDescription ToResource()
    {
        return new FileDescription { Filename = Filename, Format = Format, MimeType = MimeType, Size = Size };
    }
}
