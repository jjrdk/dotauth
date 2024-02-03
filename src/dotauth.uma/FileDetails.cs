namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the resource registration.
/// </summary>
[DataContract]
public class FileDetails : FileDescription
{
    /// <summary>
    /// Gets or sets the resource file name
    /// </summary>
    [DataMember(Name = "location")]
    public Uri Location { get; set; } = null!;

    public FileDescription ToResource()
    {
        return new FileDescription { Filename = Filename, Format = Format, MimeType = MimeType, Size = Size };
    }
}
