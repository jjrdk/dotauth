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

namespace DotAuth.Tests.Helpers;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

public sealed class GrantedTokenGeneratorHelperFixture
{
    private readonly IClientStore _clientRepositoryStub;

    public GrantedTokenGeneratorHelperFixture()
    {
        _clientRepositoryStub = Substitute.For<IClientStore>();
    }

    [Fact]
    public async Task WhenPassingNullOrWhiteSpaceThenErrorIsReturned()
    {
        var result = await _clientRepositoryStub.GenerateToken(
                new InMemoryJwksRepository(),
                string.Empty,
                Array.Empty<string>(),
                "",
                CancellationToken.None,
                userInformationPayload: null)
            .ConfigureAwait(false);

        Assert.IsType<Option<GrantedToken>.Error>(result);
    }

    [Fact]
    public async Task WhenClientDoesNotExistThenErrorIsReturned()
    {
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        var ex = Assert.IsType<Option<GrantedToken>.Error>(await _clientRepositoryStub.GenerateToken(
                new InMemoryJwksRepository(),
                "invalid_client",
                Array.Empty<string>(),
                "",
                CancellationToken.None,
                userInformationPayload: null)
            .ConfigureAwait(false));
        Assert.Equal(ErrorCodes.InvalidClient, ex.Details.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, ex.Details.Detail);
    }

    [Fact]
    public async Task When_ExpirationTime_Is_Set_Then_ExpiresInProperty_Is_Set()
    {
        var client = new Client
        {
            TokenLifetime = TimeSpan.FromSeconds(3700),
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            ClientId = "client_id",
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
            IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaSha256,
            IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
        };

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = Assert.IsType<Option<GrantedToken>.Result>(await _clientRepositoryStub.GenerateToken(
                new InMemoryJwksRepository(),
                "client_id",
                new[] { "scope" },
                "issuer",
                CancellationToken.None,
                userInformationPayload: null)
            .ConfigureAwait(false));

        Assert.Equal(3700, result.Item.ExpiresIn);
    }
}
