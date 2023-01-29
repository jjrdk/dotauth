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
    /// Gets or sets the id token for the query.
    /// </summary>
    [DataMember(Name = "id_token")]
    public string IdToken { get; set; } = "";
    
    /// <summary>
    /// Gets or sets the search terms.
    /// </summary>
    /// <value>
    /// The search terms.
    /// </value>
    [DataMember(Name = "terms")]
    public string[] Terms { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the requested resource types.
    /// </summary>
    /// <value>
    /// The requested resource types.
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
    /// Gets or sets the page size.
    /// </summary>
    /// <value>
    /// The page size of the result set.
    /// </value>
    [DataMember(Name = "count")]
    public int PageSize { get; set; } = 100;
}