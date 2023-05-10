namespace DotAuth.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DotAuth.Shared;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the jwks extensions.
/// </summary>
public static class JsonWebKeySetExtensions
{
    /// <summary>
    /// Adds the key.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    public static JsonWebKeySet AddKey(this JsonWebKeySet set, JsonWebKey key)
    {
        set.Keys.Add(key);
        return set;
    }

    /// <summary>
    /// Converts to jwks.
    /// </summary>
    /// <param name="keys">The keys.</param>
    /// <returns></returns>
    public static JsonWebKeySet? ToJwks(this IEnumerable<JsonWebKey> keys)
    {
        var set = new JsonWebKeySet();
        foreach (var key in keys)
        {
            set.Keys.Add(key);
        }

        return set;
    }

    /// <summary>
    /// Gets the encryption keys.
    /// </summary>
    /// <param name="jwks">The JWKS.</param>
    /// <returns></returns>
    public static IEnumerable<SecurityKey?> GetEncryptionKeys(this JsonWebKeySet? jwks)
    {
        return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Enc && x.KeyOps.Contains(KeyOperations.Encrypt));
    }

    /// <summary>
    /// Gets the sign keys.
    /// </summary>
    /// <param name="jwks">The JWKS.</param>
    /// <returns></returns>
    public static IEnumerable<SecurityKey> GetSignKeys(this JsonWebKeySet jwks)
    {
        return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign));
    }

    /// <summary>
    /// Gets the signing credentials.
    /// </summary>
    /// <param name="jwks">The JWKS.</param>
    /// <param name="alg">The alg.</param>
    /// <returns></returns>
    public static IEnumerable<SigningCredentials> GetSigningCredentials(this JsonWebKeySet jwks, string alg)
    {
        return jwks.Keys.Where(
                x => x.Alg == alg && x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign))
            .Select(
                webKey =>
                {
                    if (webKey.X5c == null)
                    {
                        return new SigningCredentials(webKey, alg);
                    }

                    foreach (var certString in webKey.X5c)
                    {
                        return new X509SigningCredentials(
                            new X509Certificate2(Convert.FromBase64String(certString)));
                    }

                    return new SigningCredentials(webKey, alg);
                });
    }
}