namespace DotAuth.Shared.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the generic result.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class PagedResult<T>
{
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
    public long StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>
    /// The content.
    /// </value>
    [JsonPropertyName("content")]
    public T[] Content { get; set; } = [];
}
