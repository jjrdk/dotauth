namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the search resource set request.
/// </summary>
public sealed record SearchResourceSet
{
    /// <summary>
    /// Gets or sets the id token for the query.
    /// </summary>
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = "";
    
    /// <summary>
    /// Gets or sets the search terms.
    /// </summary>
    /// <value>
    /// The search terms.
    /// </value>
    [JsonPropertyName("terms")]
    public string[] Terms { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the requested resource types.
    /// </summary>
    /// <value>
    /// The requested resource types.
    /// </value>
    [JsonPropertyName("types")]
    public string[] Types { get; set; } = [];

    /// <summary>
    /// Gets or sets the start index.
    /// </summary>
    /// <value>
    /// The start index.
    /// </value>
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    /// <value>
    /// The page size of the result set.
    /// </value>
    [JsonPropertyName("count")]
    public int PageSize { get; set; } = 100;
}
