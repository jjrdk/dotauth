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
    public JsonWebKey EncryptionKey { get; } = TestKeys.SecretKey.CreateEncryptionJwk();

    public JsonWebKey ModelEncryptionKey { get; } = TestKeys.SecretKey.CreateJwk(
        JsonWebKeyUseNames.Enc,
        KeyOperations.Decrypt,
        KeyOperations.Encrypt);

    public JsonWebKey SignatureKey { get; } = TestKeys.SecretKey.CreateSignatureJwk();

    public JsonWebKey ModelSignatureKey { get; } = TestKeys.SecretKey.CreateSignatureJwk();

    public IConfirmationCodeStore ConfirmationCodeStore { get; } = Substitute.For<IConfirmationCodeStore>();

    public ISmsClient TwilioClient { get; } = Substitute.For<ISmsClient>();

    public Func<HttpClient> Client { get; set; } = null!;

    public HttpMessageHandler ClientHandler { get; set; } = null!;
}
