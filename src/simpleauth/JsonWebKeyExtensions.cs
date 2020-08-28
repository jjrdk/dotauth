namespace SimpleAuth
{
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using SimpleAuth.Properties;

    /// <summary>
    /// Defines the JWK extension methods.
    /// </summary>
    public static class JsonWebKeyExtensions
    {
        /// <summary>
        /// Creates a <see cref="JsonWebKeySet"/> from the passed <see cref="JsonWebKey"/>.
        /// </summary>
        /// <param name="jwk"></param>
        /// <returns></returns>
        public static JsonWebKeySet ToSet(this JsonWebKey jwk)
        {
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk);
            if (jwks.Keys.Any(x => x.Kty == "oct"))
            {
                jwks.SkipUnresolvedJsonWebKeys = false;
            }

            return jwks;
        }

        /// <summary>
        /// Creates a <see cref="JsonWebKey"/> from the passed <see cref="X509Certificate2"/>.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <param name="use">The key use.</param>
        /// <param name="keyOperations">The key operations</param>
        /// <returns></returns>
        public static JsonWebKey CreateJwk(this X509Certificate2 certificate, string use, params string[] keyOperations)
        {
            JsonWebKey? jwk = null;
            if (certificate.HasPrivateKey)
            {
                var keyAlg = certificate.SignatureAlgorithm.FriendlyName ?? string.Empty;
                if (keyAlg.Contains("RSA"))
                {
                    var rsa = (RSA)certificate.PrivateKey!;
                    var parameters = rsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Kid = certificate.Thumbprint,
                        Kty = JsonWebAlgorithmsKeyTypes.RSA,
                        //Alg = keyAlg,
                        E = parameters.Exponent == null ? null : Convert.ToBase64String(parameters.Exponent),
                        N = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Modulus),
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        DP = parameters.DP == null ? null : Convert.ToBase64String(parameters.DP),
                        DQ = parameters.DQ == null ? null : Convert.ToBase64String(parameters.DQ),
                        QI = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.InverseQ!),
                        P = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.P!),
                        Q = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Q!)
                    };
                }
                else if (keyAlg.Contains("ecdsa"))
                {
                    var ecdsa = certificate.GetECDsaPrivateKey()!;
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

            jwk ??= JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(certificate));

            jwk.Use = use;
            jwk.X5t = certificate.Thumbprint;
            jwk.Kid = certificate.Thumbprint;

            foreach (var keyOperation in keyOperations)
            {
                jwk.KeyOps.Add(keyOperation);
            }

            return jwk;
        }

        /// <summary>
        /// Creates a <see cref="JsonWebKey"/> from the passed secret.
        /// </summary>
        /// <param name="key">The secret.</param>
        /// <param name="use">The key use.</param>
        /// <param name="keyOperations">The key operations</param>
        public static JsonWebKey CreateJwk(this string key, string use, params string[] keyOperations)
        {
            if (key.Length < 16)
            {
                throw new ArgumentException(Strings.Key16char, nameof(key));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var jwk = JsonWebKeyConverter.ConvertFromSymmetricSecurityKey(securityKey);
            jwk.Alg = SecurityAlgorithms.HmacSha256;
            jwk.Kty = JsonWebAlgorithmsKeyTypes.Octet;
            jwk.Use = use;
            jwk.Kid = securityKey.KeyId ?? Id.Create();
            foreach (var keyOperation in keyOperations)
            {
                jwk.KeyOps.Add(keyOperation);
            }

            return jwk;
        }

        /// <summary>
        /// Creates a signature key from the passed secret.
        /// </summary>
        /// <param name="key">The secret</param>
        /// <returns></returns>
        public static JsonWebKey CreateSignatureJwk(this string key)
        {
            return CreateJwk(key, JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify);
        }

        /// <summary>
        /// Creates a signature key from the passed <see cref="RSA"/>.
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="keyid"></param>
        /// <param name="includePrivateParameters"></param>
        /// <returns></returns>
        public static JsonWebKey CreateSignatureJwk(this RSA rsa, string keyid, bool includePrivateParameters)
        {
            return CreateJwk(
                rsa,
                keyid,
                JsonWebKeyUseNames.Sig,
                includePrivateParameters,
                KeyOperations.Sign,
                KeyOperations.Verify);
        }

        /// <summary>
        /// Creates the encryption JWK.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static JsonWebKey CreateEncryptionJwk(this string key)
        {
            return CreateJwk(key, JsonWebKeyUseNames.Enc, KeyOperations.Encrypt, KeyOperations.Decrypt);
        }

        /// <summary>
        /// Creates the encryption JWK.
        /// </summary>
        /// <param name="rsa">The RSA.</param>
        /// <param name="keyid">The keyid.</param>
        /// <param name="includePrivateParameters">if set to <c>true</c> [include private parameters].</param>
        /// <returns></returns>
        public static JsonWebKey CreateEncryptionJwk(this RSA rsa, string keyid, bool includePrivateParameters)
        {
            return CreateJwk(
                rsa,
                keyid,
                JsonWebKeyUseNames.Enc,
                includePrivateParameters,
                KeyOperations.Encrypt,
                KeyOperations.Decrypt);
        }

        /// <summary>
        /// Reads the JWK.
        /// </summary>
        /// <param name="rsa">The RSA.</param>
        /// <param name="jwk">The JWK.</param>
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

        /// <summary>
        /// Creates the JWK.
        /// </summary>
        /// <param name="rsa">The RSA.</param>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="use">The use.</param>
        /// <param name="includePrivateParameters">if set to <c>true</c> [include private parameters].</param>
        /// <param name="keyops">The keyops.</param>
        /// <returns></returns>
        public static JsonWebKey CreateJwk(
            this RSA rsa,
            string keyId,
            string use,
            bool includePrivateParameters = false,
            params string[] keyops)
        {
            var parameters = rsa.ExportParameters(includePrivateParameters);
            var key = new RsaSecurityKey(parameters);
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            jwk.Use = use;
            jwk.Kid = keyId;
            jwk.Alg = SecurityAlgorithms.RsaSha256;
            jwk.CryptoProviderFactory = CryptoProviderFactory.Default;
            foreach (var keyop in keyops)
            {
                jwk.KeyOps.Add(keyop);
            }

            return jwk;
        }
    }
}
