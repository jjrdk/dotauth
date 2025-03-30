namespace DotAuth.Shared.Responses;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the search auth policies response.
/// </summary>
public sealed record SearchAuthPoliciesResponse
{
    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>
    /// The content.
    /// </value>
    [JsonPropertyName("content")]
    public PolicyResponse[] Content { get; set; } = [];

    /// <summary>
    /// Gets or sets the total results.
    /// </summary>
    /// <value>
    /// The total results.
    /// </value>
    [JsonPropertyName("count")]
    public long TotalResults { get; set; }

    /// <summary>
    /// Gets or sets the start index.
    /// </summary>
    /// <value>
    /// The start index.
    /// </value>
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }
}
