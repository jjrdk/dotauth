namespace DotAuth.Shared.Requests;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the client search request.
/// </summary>
public sealed record SearchClientsRequest
{
    /// <summary>
    /// Gets or sets the client names.
    /// </summary>
    /// <value>
    /// The client names.
    /// </value>
    [JsonPropertyName("client_names")]
    public string[] ClientNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the client ids.
    /// </summary>
    /// <value>
    /// The client ids.
    /// </value>
    [JsonPropertyName("client_ids")]
    public string[] ClientIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the client types.
    /// </summary>
    /// <value>
    /// The client types.
    /// </value>
    [JsonPropertyName("client_types")]
    public string[] ClientTypes { get; set; } = [];

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
    public int NbResults { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SearchClientsRequest"/> is descending.
    /// </summary>
    /// <value>
    ///   <c>true</c> if descending; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("order")]
    public bool Descending { get; set; }
}
