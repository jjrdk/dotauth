namespace DotAuth.Shared.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the resource set search result.
/// </summary>
public record ResourceSetDescription
{
    /// <summary>
    /// Gets or sets the id of the resource set.
    /// </summary>
    [JsonPropertyName("_id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the resource description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>
    /// Gets or sets the icon URI.
    /// </summary>
    /// <value>
    /// The icon URI.
    /// </value>
    [JsonPropertyName("icon_uri")]
    public Uri? IconUri { get; set; }
}
