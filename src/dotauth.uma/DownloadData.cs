namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public class DownloadData
{
    /// <summary>
    /// Gets or sets the resource mime type.
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets or sets the content of the resource.
    /// </summary>
    [JsonPropertyName("content")]
    public byte[]? Content { get; init; }

    public string ToDataString()
    {
        return Content == null || MimeType == null ? "" : $"data:{MimeType};base64,{Convert.ToBase64String(Content)}";
    }
}
