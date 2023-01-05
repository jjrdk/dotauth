namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the search resource set request.
/// </summary>
[DataContract]
public sealed record SearchResourceSet
{
    /// <summary>
    /// Gets or sets the ids.
    /// </summary>
    /// <value>
    /// The ids.
    /// </value>
    [DataMember(Name = "terms")]
    public string[] Terms { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the types.
    /// </summary>
    /// <value>
    /// The types.
    /// </value>
    [DataMember(Name = "types")]
    public string[] Types { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the start index.
    /// </summary>
    /// <value>
    /// The start index.
    /// </value>
    [DataMember(Name = "start_index")]
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the total results.
    /// </summary>
    /// <value>
    /// The total results.
    /// </value>
    [DataMember(Name = "count")]
    public int TotalResults { get; set; }
}