namespace SimpleAuth
{
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Logging;

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
                    var rsa = (RSA)certificate.PrivateKey;
                    var parameters = rsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Kid = certificate.Thumbprint,
                        Kty = JsonWebAlgorithmsKeyTypes.RSA,
                        Alg = keyAlg,
                        E = parameters.Exponent == null ? null : Convert.ToBase64String(parameters.Exponent),
                        N = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Modulus),
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        DP = parameters.DP == null ? null : Convert.ToBase64String(parameters.DP),
                        DQ = parameters.DQ == null ? null : Convert.ToBase64String(parameters.DQ),
                        QI = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.InverseQ),
                        P = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.P),
                        Q = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Q)
                    };
                }
                else if (keyAlg.Contains("ecdsa"))
                {
                    var ecdsa = certificate.GetECDsaPrivateKey();
                    var parameters = ecdsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                        Alg = keyAlg,
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        Crv = parameters.Curve.Hash.ToString(),
                        X = parameters.Q.X.ToBase64Simplified(),
                        Y = parameters.Q.Y.ToBase64Simplified()
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
            jwk.Kty = JsonWebAlgorithmsKeyTypes.Octet;
            jwk.Use = use;
            jwk.Kid = securityKey.KeyId ?? Id.Create();
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