namespace DotAuth.Shared.Models;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the resource set search result.
/// </summary>
[DataContract]
public record ResourceSetDescription
{
    /// <summary>
    /// Gets or sets the id of the resource set.
    /// </summary>
    [DataMember(Name = "_id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [DataMember(Name = "name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the resource description.
    /// </summary>
    [DataMember(Name = "description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [DataMember(Name = "type")]
    public string Type { get; set; } = "";

    /// <summary>
    /// Gets or sets the icon URI.
    /// </summary>
    /// <value>
    /// The icon URI.
    /// </value>
    [DataMember(Name = "icon_uri")]
    public Uri? IconUri { get; set; }
}