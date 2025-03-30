namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines an external resource location
/// </summary>
public record ResourceRedirection : ResourceResult
{
    /// <summary>
    /// Gets the resource location.
    /// </summary>
    [JsonPropertyName("location")]
    public Uri Location { get; set; } = null!;
}
