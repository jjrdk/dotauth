namespace DotAuth.Shared.Models;

using System.Security.Claims;
using System.Text.Json.Serialization;

/// <summary>
/// Defines the posted claim.
/// </summary>
public sealed record ClaimData
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    [JsonPropertyName("value")]
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
