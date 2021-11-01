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
    using System;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Shared;
    using SimpleAuth;
    using System.Net.Http;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms;

    public class SharedContext
    {
        public SharedContext()
        {
            SignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
            ModelSignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
            EncryptionKey = TestKeys.SecretKey.CreateEncryptionJwk();
            ModelEncryptionKey = TestKeys.SuperSecretKey.CreateJwk(
                JsonWebKeyUseNames.Enc,
                KeyOperations.Decrypt,
                KeyOperations.Encrypt);
            ConfirmationCodeStore = new Mock<IConfirmationCodeStore>();
            TwilioClient = new Mock<ISmsClient>();
        }

        public JsonWebKey EncryptionKey { get; }

        public JsonWebKey ModelEncryptionKey { get; }

        public JsonWebKey SignatureKey { get; }

        public JsonWebKey ModelSignatureKey { get; }

        public Mock<IConfirmationCodeStore> ConfirmationCodeStore { get; }

        public Mock<ISmsClient> TwilioClient { get; }

        public Func<HttpClient> Client { get; set; }

        public HttpMessageHandler ClientHandler { get; set; }
    }
}
