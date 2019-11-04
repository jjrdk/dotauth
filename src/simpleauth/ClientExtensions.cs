namespace SimpleAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal static class ClientExtensions
    {
        public static async Task<TokenValidationParameters> CreateValidationParameters(this Client client, IJwksStore jwksStore, string audience = null, string issuer = null)
        {
            var signingKeys = client.JsonWebKeys.GetSigningKeys();
            if (signingKeys.Count == 0)
            {
                var keys = await (client.IdTokenSignedResponseAlg == null
                    ? jwksStore.GetDefaultSigningKey()
                    : jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg)).ConfigureAwait(false);

                signingKeys = new List<SecurityKey> { keys.Key };
            }
            var encryptionKeys = client.JsonWebKeys.GetEncryptionKeys().ToArray();
            if (encryptionKeys.Length == 0 && client.IdTokenEncryptedResponseAlg != null)
            {
                var key = await jwksStore.GetEncryptionKey(client.IdTokenEncryptedResponseAlg).ConfigureAwait(false);

                encryptionKeys = new[] { key };
            }
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKeys = signingKeys,
                TokenDecryptionKeys = encryptionKeys
            };
            if (audience != null)
            {
                parameters.ValidAudience = audience;
            }
            else
            {
                parameters.ValidateAudience = false;
            }
            if (issuer != null)
            {
                parameters.ValidIssuer = issuer;
            }
            else
            {
                parameters.ValidateIssuer = false;
            }

            return parameters;
        }


        public static IList<SecurityKey> GetAllSigningKeys(this JsonWebKeySet keys)
        {
            List<SecurityKey> securityKeyList = new List<SecurityKey>();
            foreach (JsonWebKey key1 in (IEnumerable<JsonWebKey>)keys.Keys)
            {
                if (!string.IsNullOrEmpty(key1.Use) && !key1.Use.Equals("sig", StringComparison.Ordinal))
                {
                    LogHelper.LogInformation(LogHelper.FormatInvariant("IDX10808: The 'use' parameter of a JsonWebKey: '{0}' was expected to be 'sig' or empty, but was '{1}'.", (object)key1, (object)key1.Use));

                }
                else if ("RSA".Equals(key1.Kty, StringComparison.Ordinal))
                {
                    bool flag = true;
                    if ((key1.X5c == null || key1.X5c.Count == 0) && (string.IsNullOrEmpty(key1.E) && string.IsNullOrEmpty(key1.N)))
                    {
                        flag = false;
                    }
                    else
                    {
                        if (key1.X5c != null && key1.X5c.Count != 0)
                        {
                            SecurityKey key2;
                            //if (JsonWebKeyConverter.TryConvertToX509SecurityKey(key1, out key2))
                            //  securityKeyList.Add(key2);
                            //else
                            //  flag = false;
                        }
                        if (!string.IsNullOrEmpty(key1.E) && !string.IsNullOrEmpty(key1.N))
                        {
                            SecurityKey key2;
                            //if (JsonWebKeyConverter.TryCreateToRsaSecurityKey(key1, out key2))
                            //  securityKeyList.Add(key2);
                            //else
                            //  flag = false;
                        }
                    }
                    //if (!flag && !this.SkipUnresolvedJsonWebKeys)
                    //  securityKeyList.Add((SecurityKey) key1);
                }
                else if ("EC".Equals(key1.Kty, StringComparison.Ordinal))
                {
                    SecurityKey key2;
                    //if (JsonWebKeyConverter.TryConvertToECDsaSecurityKey(key1, out key2))
                    //  securityKeyList.Add(key2);
                    //else if (!this.SkipUnresolvedJsonWebKeys)
                    //  securityKeyList.Add((SecurityKey) key1);
                }
                else
                {
                    LogHelper.LogInformation(LogHelper.FormatInvariant("IDX10810: Unable to convert the JsonWebKey: '{0}' to a X509SecurityKey, RsaSecurityKey or ECDSASecurityKey.", (object)key1));
                    //if (!this.SkipUnresolvedJsonWebKeys)
                    //  securityKeyList.Add((SecurityKey) key1);
                }
            }
            return (IList<SecurityKey>)securityKeyList;
        }
    }
}