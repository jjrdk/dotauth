namespace SimpleAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.IdentityModel.Tokens;
    using Shared;

    internal static class JsonWebKeySetExtensions
    {
        public static JsonWebKeySet AddKey(this JsonWebKeySet set, JsonWebKey key)
        {
            set.Keys.Add(key);
            return set;
        }

        public static JsonWebKeySet ToJwks(this IEnumerable<JsonWebKey> keys)
        {
            var set = new JsonWebKeySet();
            foreach (var key in keys)
            {
                set.Keys.Add(key);
            }

            return set;
        }

        public static IEnumerable<SecurityKey> GetEncryptionKeys(this JsonWebKeySet jwks)
        {
            return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Enc && x.KeyOps.Contains(KeyOperations.Encrypt));
        }

        public static IEnumerable<SecurityKey> GetSignKeys(this JsonWebKeySet jwks)
        {
            return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign));
        }

        public static IEnumerable<SigningCredentials> GetSigningCredentials(this JsonWebKeySet jwks, string alg)
        {
            return jwks.Keys.Where(x => x.Alg == alg && x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign))
                .Select(webKey =>
                {
                    if (webKey.X5c != null)
                    {
                        foreach (var certString in webKey.X5c)
                        {
                            return new X509SigningCredentials(new X509Certificate2(Convert.FromBase64String(certString)));
                        }
                    }

                    return new SigningCredentials(webKey, alg);
                });
        }
    }
}