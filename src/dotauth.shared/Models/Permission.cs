namespace DotAuth.Shared.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the resource permission.
/// </summary>
public sealed record Permission
{
    /// <summary>
    /// Gets or sets the permitted resource set id.
    /// </summary>
    [JsonPropertyName("resource_set_id")]
    public string ResourceSetId { get; set; } = null!;

    /// <summary>
    /// <para>Gets or sets an array referencing one or more URIs of scopes to which access was granted for this resource set.</para>
    /// <para>Each scope MUST correspond to a scope that was registered by this resource server for the referenced resource set.</para>
    /// </summary>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// <para>Gets or sets an integer timestamp, measured in the number of seconds since January 1 1970 UTC,
    /// indicating when this permission will expire.</para>
    /// <para>If the property is absent, the permission does not expire.</para>
    /// </summary>
    [JsonPropertyName("exp")]
    public long? Expiry { get; set; }

    /// <summary>
    /// <para>Gets or sets Integer timestamp, measured in the number of seconds since January 1 1970 UTC,
    /// indicating when this permission was originally issued.</para>
    /// <para>If the token-level "iat" value post-dates a permission-level "iat" value, the former overrides the latter.</para>
    /// </summary>
    [JsonPropertyName("iat")]
    public long? IssuedAt { get; set; }

    /// <summary>
    /// <para>Gets or sets an integer timestamp, measured in the number of seconds since January 1 1970 UTC,
    /// indicating the time before which this permission is not valid.</para>
    /// <para>If the token-level "nbf" value post-dates a permission-level "nbf" value, the former overrides the latter.</para>
    /// </summary>
    [JsonPropertyName("nbf")]
    public long? NotBefore { get; set; }
}
