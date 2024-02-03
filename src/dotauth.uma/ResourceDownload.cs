namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the resource download model.
/// </summary>
[DataContract]
public record ResourceDownload : ResourceResult
{
    /// <summary>Gets or sets the name.</summary>
    /// <value>The name.</value>
    [DataMember(Name = "name")]
    public string? Name { get; init; }

    /// <summary>Gets or sets the resource description.</summary>
    [DataMember(Name = "description")]
    public string? Description { get; init; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    [DataMember(Name = "type")]
    public string? Type { get; init; }

    [DataMember(Name = "data")]
    public DownloadData[] Data { get; set; } = Array.Empty<DownloadData>();
}