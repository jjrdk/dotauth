namespace SimpleAuth
{
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public static class ClientExtensions
    {
        public static TokenValidationParameters CreateValidationParameters(this Client client, string audience = null, string issuer = null)
        {
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKeys = client.JsonWebKeys.GetSignKeys(),
                TokenDecryptionKeys = client.JsonWebKeys.GetEncryptionKeys()
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
    }

    public static class JwtPayloadExtensions
    {
        public static string GetClaimValue(this JwtPayload payload, string claimType)
        {
            return payload.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        public static string[] GetArrayValue(this JwtPayload payload, string claimType)
        {
            return payload.Claims
                .Where(c => c.Type == claimType && !string.IsNullOrWhiteSpace(c.Value))
                .Select(c => c.Value)
                .ToArray();
        }
    }

    public static class JsonWebKeySetExtensions
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
            return jwks.Keys.Where(x => x.Use == JsonWebKeyUseNames.Sig && x.KeyOps.Contains(KeyOperations.Sign))
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

    public static class JwtSecurityTokenExtensions
    {
        public static bool IsJweToken(this string token)
        {
            return token.Split('.').Length == 5;
        }

        public static bool IsJwsToken(this string token)
        {
            return token.Split('.').Length == 3;
        }
    }

    public static class JsonWebKeyExtensions
    {
        public static JsonWebKeySet ToSet(this JsonWebKey jwk)
        {
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk);
            return jwks;
        }

        public static JsonWebKey CreateJwk(this X509Certificate2 certificate, string use, params string[] keyOperations)
        {
            if (keyOperations == null)
            {
                throw new ArgumentNullException(nameof(keyOperations));
            }
            JsonWebKey jwk = null;
            if (certificate.HasPrivateKey)
            {
                var keyAlg = certificate.SignatureAlgorithm.FriendlyName;
                if (keyAlg.Contains("RSA"))
                {
                    var rsa = certificate.PrivateKey as RSA;
                    var xml = RsaExtensions.ToXmlString(rsa, false);
                    var parameters = xml.ToRSAParameters();
                    jwk = new JsonWebKey
                    {
                        Alg = keyAlg,
                        E = parameters.Exponent == null ? null : Convert.ToBase64String(parameters.Exponent),
                        N = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Modulus)
                    };
                    if (false)
                    {
                        jwk.D = parameters.D == null ? null : Convert.ToBase64String(parameters.D);
                        jwk.DP = parameters.DP == null ? null : Convert.ToBase64String(parameters.DP);
                        jwk.DQ = parameters.DQ == null ? null : Convert.ToBase64String(parameters.DQ);
                        jwk.QI = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.InverseQ);
                        jwk.P = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.P);
                        jwk.Q = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Q);
                    }
                }
                else if (keyAlg.Contains("ecdsa"))
                {
                    var ecdsa = certificate.GetECDsaPrivateKey();
                    var parameters = ecdsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Alg = keyAlg,
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        //Q = parameters.Q == null ? null:Convert.ToBase64String(parameters.Q),
                    };
                }
            }

            if (jwk == null)
            {
                jwk = JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(certificate));
            }

            jwk.Use = use;
            jwk.X5t = certificate.Thumbprint;
            jwk.Kid = certificate.Thumbprint;

            foreach (var keyOperation in keyOperations)
            {
                jwk.KeyOps.Add(keyOperation);
            }

            return jwk;
        }

        public static JsonWebKey CreateJwk(this string key, string use, params string[] keyOperations)
        {
            if (key.Length < 16)
            {
                throw new ArgumentException("Key must be at least 16 characters.", nameof(key));
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var jwk = JsonWebKeyConverter.ConvertFromSymmetricSecurityKey(securityKey);
            jwk.Use = use;
            jwk.Kid = securityKey.KeyId ?? Guid.NewGuid().ToString("N");
            if (keyOperations != null)
            {
                foreach (var keyOperation in keyOperations)
                {
                    jwk.KeyOps.Add(keyOperation);
                }
            }
            return jwk;
        }

        public static JsonWebKey CreateSignatureJwk(this string key)
        {
            return CreateJwk(key, JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify);
        }

        public static JsonWebKey CreateEncryptionJwk(this string key)
        {
            return CreateJwk(key, JsonWebKeyUseNames.Enc, KeyOperations.Encrypt, KeyOperations.Decrypt);
        }

        public static void ReadJwk(this RSA rsa, JsonWebKey jwk)
        {
            var parameters = new RSAParameters
            {
                D = jwk.D == null ? null : Convert.FromBase64String(jwk.D),
                DP = jwk.DP == null ? null : Convert.FromBase64String(jwk.DP),
                DQ = jwk.DQ == null ? null : Convert.FromBase64String(jwk.DQ),
                Exponent = jwk.E == null ? null : Convert.FromBase64String(jwk.E),
                Modulus = jwk.N == null ? null : Convert.FromBase64String(jwk.N),
                InverseQ = jwk.QI == null ? null : Convert.FromBase64String(jwk.QI),
                P = jwk.P == null ? null : Convert.FromBase64String(jwk.P),
                Q = jwk.Q == null ? null : Convert.FromBase64String(jwk.Q),
            };
            rsa.ImportParameters(parameters);
        }

        public static JsonWebKey CreateJwk(this RSA rsa, string use, IEnumerable<string> keyops, bool includePrivateParameters)
        {
            var parameters = rsa.ExportParameters(includePrivateParameters);
            var key = new RsaSecurityKey(parameters);
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            jwk.Use = use;
            foreach (var keyop in keyops)
            {
                jwk.KeyOps.Add(keyop);
            }
            return jwk;
        }
    }
}