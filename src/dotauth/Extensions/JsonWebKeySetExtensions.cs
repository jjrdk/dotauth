namespace DotAuth.Extensions;

using System.Collections.Generic;
using System.Linq;
using DotAuth.Shared;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the jwks extensions.
/// </summary>
public static class JsonWebKeySetExtensions
{
    /// <summary>
    /// Converts to jwks.
    /// </summary>
    /// <param name="keys">The keys.</param>
    /// <returns></returns>
    public static JsonWebKeySet ToJwks(this IEnumerable<JsonWebKey> keys)
    {
        var set = new JsonWebKeySet();
        foreach (var key in keys)
        {
            set.Keys.Add(key);
        }

        return set;
    }

    /// <param name="jwks">The JWKS.</param>
    extension(JsonWebKeySet jwks)
    {
        /// <summary>
        /// Adds the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public JsonWebKeySet AddKey(JsonWebKey key)
        {
            jwks.Keys.Add(key);
            return jwks;
        }

        /// <summary>
        /// Gets the encryption keys.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SecurityKey?> GetEncryptionKeys()
        {
            return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Enc && x.KeyOps.Contains(KeyOperations.Encrypt));
        }

        /// <summary>
        /// Gets the sign keys.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SecurityKey> GetSignKeys()
        {
            return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign));
        }
    }
}
