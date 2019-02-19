// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.AcceptanceTests
{
    using System.Net.Http;
    using System.Security.Cryptography;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth;

    public class SharedContext
    {
        public SharedContext()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                SignatureKey = rsa.CreateSignatureJwk("1", true);
                //new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaSha256,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Sign,
                //        KeyOperations.Verify
                //    },
                //    Kid = "1",
                //    Kty = KeyType.RSA,
                //    Use = Use.Sig,
                //    SerializedKey = serializedRsa,
                //};
                ModelSignatureKey = rsa.CreateSignatureJwk("2", true);
                //    new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaSha256,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Encrypt,
                //        KeyOperations.Decrypt
                //    },
                //    Kid = "2",
                //    Kty = KeyType.RSA,
                //    Use = Use.Sig,
                //    SerializedKey = serializedRsa,
                //};
                EncryptionKey = rsa.CreateEncryptionJwk("3", true);
                //    new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaPKCS1,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Decrypt,
                //        KeyOperations.Encrypt
                //    },
                //    Kid = "3",
                //    Kty = KeyType.RSA,
                //    Use = Use.Enc,
                //    SerializedKey = serializedRsa,
                //};
                ModelEncryptionKey = rsa.CreateEncryptionJwk("4", true);
                //    new JsonWebKey
                //{
                //    Alg = SecurityAlgorithms.RsaPKCS1,
                //    KeyOps = new[]
                //    {
                //        KeyOperations.Encrypt,
                //        KeyOperations.Decrypt
                //    },
                //    Kid = "4",
                //    Kty = KeyType.RSA,
                //    Use = Use.Enc,
                //    SerializedKey = serializedRsa,
                //};
            }
        }

        public JsonWebKey EncryptionKey { get; }
        public JsonWebKey ModelEncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public JsonWebKey ModelSignatureKey { get; }
        public HttpClient Client { get; set; }
    }
}
