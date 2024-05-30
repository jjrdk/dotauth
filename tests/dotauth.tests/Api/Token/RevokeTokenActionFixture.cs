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

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token.Actions;
using DotAuth.Controllers;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

public sealed class RevokeTokenActionFixture
{
    private readonly IClientStore _clientStore;
    private readonly ITokenStore _grantedTokenRepositoryStub;
    private readonly RevokeTokenAction _revokeTokenAction;

    public RevokeTokenActionFixture()
    {
        _clientStore = Substitute.For<IClientStore>();
        _grantedTokenRepositoryStub = Substitute.For<ITokenStore>();
        _revokeTokenAction = new RevokeTokenAction(
            _clientStore,
            _grantedTokenRepositoryStub,
            new InMemoryJwksRepository(),
            Substitute.For<ILogger<TokenController>>());
    }

    [Fact]
    public async Task WhenClientDoesNotExistThenErrorIsReturned()
    {
        var parameter = new RevokeTokenParameter { Token = "access_token" };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var error = Assert.IsType<Option.Error>(await _revokeTokenAction
            .Execute(parameter, null, null, "", CancellationToken.None));
        Assert.Equal(ErrorCodes.InvalidClient, error.Details.Title);
    }

    [Fact]
    public async Task When_Token_Does_Not_Exist_Then_Exception_Is_Returned()
    {
        var clientid = "clientid";
        var clientsecret = "secret";
        var parameter = new RevokeTokenParameter { Token = "access_token" };

        var client = new Client
        {
            ClientId = clientid,
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret }]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _grantedTokenRepositoryStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GrantedToken?)null);
        _grantedTokenRepositoryStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((GrantedToken?)null);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientid}:{clientsecret}".Base64Encode());
        var result = Assert.IsType<Option.Error>( await _revokeTokenAction.Execute(
                parameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None));

        Assert.Equal("invalid_token", result?.Details.Title);
    }

    [Fact]
    public async Task When_Invalidating_Refresh_Token_Then_GrantedTokenChildren_Are_Removed()
    {
        var clientid = "clientid";
        var clientsecret = "secret";
        var parent = new GrantedToken { ClientId = clientid, RefreshToken = "refresh_token" };

        var parameter = new RevokeTokenParameter { Token = "refresh_token" };

        var client = new Client
        {
            ClientId = clientid,
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret }]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _grantedTokenRepositoryStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((GrantedToken?)null));
        _grantedTokenRepositoryStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(parent);
        _grantedTokenRepositoryStub.RemoveAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientid}:{clientsecret}".Base64Encode());
        await _revokeTokenAction.Execute(parameter, authenticationHeader, null, "", CancellationToken.None);

        await _grantedTokenRepositoryStub.Received()
            .RemoveRefreshToken(parent.RefreshToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Invalidating_Access_Token_Then_GrantedToken_Is_Removed()
    {
        var clientId = "clientid";
        var clientSecret = "clientsecret";
        var grantedToken = new GrantedToken { ClientId = clientId, AccessToken = "access_token" };
        var parameter = new RevokeTokenParameter { Token = "access_token" };

        var client = new Client
        {
            ClientId = clientId,
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret }]
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _grantedTokenRepositoryStub.GetAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(grantedToken);
        _grantedTokenRepositoryStub.GetRefreshToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((GrantedToken?)null));
        _grantedTokenRepositoryStub.RemoveAccessToken(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        await _revokeTokenAction.Execute(parameter, authenticationHeader, null, "", CancellationToken.None);

        await _grantedTokenRepositoryStub.Received()
            .RemoveAccessToken(grantedToken.AccessToken, Arg.Any<CancellationToken>());
    }
}
