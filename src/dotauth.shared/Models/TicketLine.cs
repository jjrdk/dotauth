namespace DotAuth.Shared.Models;

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the ticket line content.
/// </summary>
public sealed record TicketLine
{
    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the resource set identifier.
    /// </summary>
    /// <value>
    /// The resource set identifier.
    /// </value>
    [JsonPropertyName("resource_id")]
    public string ResourceSetId { get; set; } = null!;
}
