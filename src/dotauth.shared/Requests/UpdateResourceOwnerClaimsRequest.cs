namespace DotAuth.Shared.Requests;

using System;
using System.Runtime.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the request to update resource owner claims.
/// </summary>
[DataContract]
public sealed record UpdateResourceOwnerClaimsRequest
{
    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    /// <value>
    /// The login.
    /// </value>
    [DataMember(Name = "sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    /// <value>
    /// The claims.
    /// </value>
    [DataMember(Name = "claims")]
    public ClaimData[] Claims { get; set; } = Array.Empty<ClaimData>();
}