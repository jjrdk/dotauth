namespace DotAuth.Uma;

using System.Runtime.Serialization;

/// <summary>
/// Defines the slim resource.
/// </summary>
[DataContract]
public class FileDescription
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    [DataMember(Name = "filename")]
    public string Filename { get; set; } = null!;

    /// <summary>
    /// Gets or sets the format of the content.
    /// </summary>
    [DataMember(Name = "format")]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the resource mime type.
    /// </summary>
    [DataMember(Name = "mime_type")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the byte size of the resource.
    /// </summary>
    [DataMember(Name = "size")]
    public long Size { get; set; }
}
