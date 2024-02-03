namespace DotAuth.Uma;

using System.IO;
using System.Text;

/// <summary>
/// Defines the uploaded file data.
/// </summary>
public class DataContent
{
    /// <summary>
    /// Gets or sets the subject of user who uploaded the file.
    /// </summary>
    public required string Owner { get; init; } = null!;

    /// <summary>
    /// Gets or sets the file encoding.
    /// </summary>
    public required Encoding Encoding { get; init; }

    /// <summary>
    /// Gets or sets the file content.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or sets the mime type.
    /// </summary>
    public required string MimeType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the size of the data file.
    /// </summary>
    public long Size { get; init; }
}