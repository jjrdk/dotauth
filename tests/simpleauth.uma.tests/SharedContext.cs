namespace SimpleAuth.Uma.Tests
{
    using System.Security.Cryptography;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared;

    public class SharedContext
    {
        private const string SecretKey = "verylongsecretkey";
        public SharedContext()
        {
                SignatureKey = SecretKey.CreateSignatureJwk();
                //    new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaSha256,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Sign,
                //        KeyOperations.Verify
                //    },
                //    Kid = "11",
                //    Kty = KeyType.RSA,
                //    Use = Use.Sig,
                //    SerializedKey = serializedRsa,
                //};
                EncryptionKey = SecretKey.CreateEncryptionJwk();
                //    new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaPKCS1,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Decrypt,
                //        KeyOperations.Encrypt
                //    },
                //    Kid = "10",
                //    Kty = KeyType.RSA,
                //    Use = Use.Enc,
                //    SerializedKey = serializedRsa,
                //};
        }

        public JsonWebKey EncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
    }
}
