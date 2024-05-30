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

namespace DotAuth.Tests.Api.Token;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token.Actions;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Tests.Helpers;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
{
    private readonly ITokenStore _tokenStoreStub;
    private readonly IClientStore _clientStore;
    private readonly GetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

    public GetTokenByRefreshTokenGrantTypeActionFixture()
    {
        IdentityModelEventSource.ShowPII = true;
        _tokenStoreStub = Substitute.For<ITokenStore>();
        _clientStore = Substitute.For<IClientStore>();
        _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
            Substitute.For<IEventPublisher>(),
            _tokenStoreStub,
            new InMemoryJwksRepository(),
            new InMemoryResourceOwnerRepository(string.Empty),
            _clientStore);
    }

    [Fact]
    public async Task When_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
    {
        var parameter = new RefreshTokenGrantTypeParameter();

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByRefreshTokenGrantTypeAction.Execute(
            parameter,
            null,
            null,
            "",
            CancellationToken.None)
        );

        Assert.Equal(ErrorCodes.InvalidClient, result.Details.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, result.Details.Detail);
    }

    [Fact]
    public async Task When_Client_Does_Not_Support_GrantType_RefreshToken_Then_Error_Is_Returned()
    {
        var parameter = new RefreshTokenGrantTypeParameter();
        var client = new Client
        {
            ClientId = "id",
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" }],
            GrantTypes = [GrantTypes.AuthorizationCode]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByRefreshTokenGrantTypeAction.Execute(
            parameter,
            authenticationHeader,
            null,
            "",
            CancellationToken.None)
        );

        Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheGrantType, "id", GrantTypes.RefreshToken),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
    {
        var parameter = new RefreshTokenGrantTypeParameter();
        var client = new Client
        {
            ClientId = "id",
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" }],
            GrantTypes = [GrantTypes.RefreshToken]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _tokenStoreStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GrantedToken?)null);

        var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
        var response = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByRefreshTokenGrantTypeAction.Execute(
            parameter,
            authenticationHeader,
            null,
            "",
            CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidGrant, response.Details.Title);
        Assert.Equal(Strings.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer, response.Details.Detail);
    }

    [Fact]
    public async Task When_RefreshToken_Is_Not_Issued_By_The_Same_Client_Then_Error_Is_Returned()
    {
        var parameter = new RefreshTokenGrantTypeParameter();
        var client = new Client
        {
            ClientId = "id",
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" }],
            GrantTypes = [GrantTypes.RefreshToken]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _tokenStoreStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GrantedToken { ClientId = "differentId" });

        var authenticationValue = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByRefreshTokenGrantTypeAction.Execute(
            parameter,
            authenticationValue,
            null,
            "issuer",
            CancellationToken.None)
        );

        Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
        Assert.Equal(Strings.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer, result.Details.Detail);
    }

    [Fact]
    public async Task When_Requesting_Token_Then_New_One_Is_Generated()
    {
        var parameter = new RefreshTokenGrantTypeParameter { ClientId = "id", RefreshToken = "abc" };
        var grantedToken = new GrantedToken { IdTokenPayLoad = new JwtPayload(), ClientId = "id", Scope = "scope" };
        var client = new Client
        {
            ClientId = "id",
            JsonWebKeys =
                TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" }],
            GrantTypes = [GrantTypes.RefreshToken]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _tokenStoreStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(grantedToken);

        var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
        await _getTokenByRefreshTokenGrantTypeAction.Execute(
                parameter,
                authenticationHeader,
                null,
                "issuer",
                CancellationToken.None)
            ;

        await _tokenStoreStub.Received().AddToken(Arg.Any<GrantedToken>(), Arg.Any<CancellationToken>());
    }
}
