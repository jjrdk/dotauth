namespace DotAuth.Shared.Models;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the ticket line content.
/// </summary>
[DataContract]
public sealed record TicketLine
{
    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    [DataMember(Name = "scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the resource set identifier.
    /// </summary>
    /// <value>
    /// The resource set identifier.
    /// </value>
    [DataMember(Name = "resource_id")]
    public string ResourceSetId { get; set; } = null!;
}