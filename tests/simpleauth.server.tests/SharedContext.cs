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

namespace SimpleAuth.Server.Tests
{
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Shared;
    using SimpleAuth;
    using System.Net.Http;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms;

    public class SharedContext
    {
        public SharedContext()
        {
            SignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
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
            ModelSignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
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
            EncryptionKey = TestKeys.SecretKey.CreateEncryptionJwk();
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
            ModelEncryptionKey = TestKeys.SuperSecretKey.CreateJwk(
                JsonWebKeyUseNames.Enc,
                KeyOperations.Decrypt,
                KeyOperations.Encrypt);
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
            ConfirmationCodeStore = new Mock<IConfirmationCodeStore>();
            TwilioClient = new Mock<ISmsClient>();
        }

        public JsonWebKey EncryptionKey { get; }
        public JsonWebKey ModelEncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public JsonWebKey ModelSignatureKey { get; }
        public Mock<IConfirmationCodeStore> ConfirmationCodeStore { get; }
        public Mock<ISmsClient> TwilioClient { get; }
        public HttpClient Client { get; set; }
    }
}
