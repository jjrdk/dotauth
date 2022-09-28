namespace SimpleAuth.Stores.Marten;

using System;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the JWK container.
/// </summary>
public sealed class JsonWebKeyContainer
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the JWK.
    /// </summary>
    public JsonWebKey Jwk { get; set; } = null!;

    /// <summary>
    /// Create a container instance from a key.
    /// </summary>
    /// <param name="key">The JWK to contain.</param>
    /// <returns>A <see cref="JsonWebKeyContainer"/> instance.</returns>
    public static JsonWebKeyContainer Create(JsonWebKey key)
    {
        return new JsonWebKeyContainer
        {
            Id = Guid.NewGuid().ToString("N"),
            Jwk = key
        };
    }
}