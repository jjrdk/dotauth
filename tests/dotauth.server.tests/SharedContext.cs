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

namespace DotAuth.Server.Tests;

using System;
using System.Net.Http;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Sms;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

public sealed class SharedContext
{
    public SharedContext()
    {
        SignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
        ModelSignatureKey = TestKeys.SecretKey.CreateSignatureJwk();
        EncryptionKey = TestKeys.SecretKey.CreateEncryptionJwk();
        ModelEncryptionKey = TestKeys.SecretKey.CreateJwk(
            JsonWebKeyUseNames.Enc,
            KeyOperations.Decrypt,
            KeyOperations.Encrypt);
        ConfirmationCodeStore = Substitute.For<IConfirmationCodeStore>();
        TwilioClient = Substitute.For<ISmsClient>();
    }

    public JsonWebKey EncryptionKey { get; }

    public JsonWebKey ModelEncryptionKey { get; }

    public JsonWebKey SignatureKey { get; }

    public JsonWebKey ModelSignatureKey { get; }

    public IConfirmationCodeStore ConfirmationCodeStore { get; }

    public ISmsClient TwilioClient { get; }

    public Func<HttpClient> Client { get; set; }

    public HttpMessageHandler ClientHandler { get; set; }
}
