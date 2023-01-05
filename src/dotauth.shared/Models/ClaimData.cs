namespace DotAuth.Shared.Models;

using System.Runtime.Serialization;
using System.Security.Claims;

/// <summary>
/// Defines the posted claim.
/// </summary>
[DataContract]
public sealed record ClaimData
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [DataMember(Name = "type")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    [DataMember(Name = "value")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Performs an implicit conversion from <see cref="Claim"/> to <see cref="ClaimData"/>.
    /// </summary>
    /// <param name="claim"></param>
    public static ClaimData FromClaim(Claim claim)
    {
        return new ClaimData { Type = claim.Type, Value = claim.Value };
    }
}