namespace DotAuth.Shared.Requests;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the search scopes request.
/// </summary>
public sealed record SearchScopesRequest
{
    /// <summary>
    /// Gets or sets the scope types.
    /// </summary>
    /// <value>
    /// The scope types.
    /// </value>
    [JsonPropertyName("types")]
    public string[] ScopeTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the scope names.
    /// </summary>
    /// <value>
    /// The scope names.
    /// </value>
    [JsonPropertyName("names")]
    public string[] ScopeNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the start index.
    /// </summary>
    /// <value>
    /// The start index.
    /// </value>
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the nb results.
    /// </summary>
    /// <value>
    /// The nb results.
    /// </value>
    [JsonPropertyName("count")]
    public int NbResults { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SearchScopesRequest"/> is descending.
    /// </summary>
    /// <value>
    ///   <c>true</c> if descending; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("order")]
    public bool Descending { get; set; }
}
