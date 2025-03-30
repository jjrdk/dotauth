namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the resource download model.
/// </summary>
public record ResourceDownload : ResourceResult
{
    /// <summary>Gets or sets the name.</summary>
    /// <value>The name.</value>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets or sets the resource description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("data")]
    public DownloadData[] Data { get; set; } = [];
}
