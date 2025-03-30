namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the search auth policy query.
/// </summary>
public sealed record SearchAuthPolicies
{
    /// <summary>
    /// Gets or sets the ids.
    /// </summary>
    /// <value>
    /// The ids.
    /// </value>
    [JsonPropertyName("ids")]
    public string[] Ids { get; set; } = [];

    /// <summary>
    /// Gets or sets the start index.
    /// </summary>
    /// <value>
    /// The start index.
    /// </value>
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the total results.
    /// </summary>
    /// <value>
    /// The total results.
    /// </value>
    [JsonPropertyName("count")]
    public int TotalResults { get; set; }
}
