namespace DotAuth.Shared.Models;

using System.Runtime.Serialization;

/// <summary>
/// Defines the client secret.
/// </summary>
[DataContract]
public sealed record ClientSecret
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [DataMember(Name = "type")]
    public ClientSecretTypes Type { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    [DataMember(Name = "value")]
    public string Value { get; set; } = null!;
}