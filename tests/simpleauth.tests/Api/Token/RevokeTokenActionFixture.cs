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

namespace SimpleAuth.Tests.Api.Token
{
    using Moq;
    using Parameters;
    using Shared.Models;
    using SimpleAuth.Api.Token.Actions;
    using System;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class RevokeTokenActionFixture
    {
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<ITokenStore> _grantedTokenRepositoryStub;
        private readonly RevokeTokenAction _revokeTokenAction;

        public RevokeTokenActionFixture()
        {
            _clientStore = new Mock<IClientStore>();
            _grantedTokenRepositoryStub = new Mock<ITokenStore>();
            _revokeTokenAction = new RevokeTokenAction(_clientStore.Object, _grantedTokenRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _revokeTokenAction.Execute(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _revokeTokenAction.Execute(
                        new RevokeTokenParameter(),
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            var parameter = new RevokeTokenParameter {Token = "access_token"};

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client) null);

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _revokeTokenAction.Execute(parameter, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
        }

        [Fact]
        public async Task When_Token_Does_Not_Exist_Then_Exception_Is_Returned()
        {
            var clientid = "clientid";
            var clientsecret = "secret";
            var parameter = new RevokeTokenParameter {Token = "access_token"};

            var client = new Client
            {
                ClientId = clientid,
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = clientsecret}}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken) null));
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken) null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientid}:{clientsecret}".Base64Encode());
            var result = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _revokeTokenAction.Execute(
                        parameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal("invalid_token", result.Code);
        }

        [Fact]
        public async Task When_Invalidating_Refresh_Token_Then_GrantedTokenChildren_Are_Removed()
        {
            var clientid = "clientid";
            var clientsecret = "secret";
            var parent = new GrantedToken {ClientId = clientid, RefreshToken = "refresh_token"};

            var parameter = new RevokeTokenParameter {Token = "refresh_token"};

            var client = new Client
            {
                ClientId = clientid,
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = clientsecret}}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken) null));
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parent);
            _grantedTokenRepositoryStub
                .Setup(g => g.RemoveAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientid}:{clientsecret}".Base64Encode());
            await _revokeTokenAction.Execute(parameter, authenticationHeader, null, null, CancellationToken.None)
                .ConfigureAwait(false);

            _grantedTokenRepositoryStub.Verify(
                g => g.RemoveRefreshToken(parent.RefreshToken, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task When_Invalidating_Access_Token_Then_GrantedToken_Is_Removed()
        {
            var clientId = "clientid";
            var clientSecret = "clientsecret";
            var grantedToken = new GrantedToken {ClientId = clientId, AccessToken = "access_token"};
            var parameter = new RevokeTokenParameter {Token = "access_token"};

            var client = new Client
            {
                ClientId = clientId,
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = clientSecret}}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(grantedToken);
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken) null));
            _grantedTokenRepositoryStub
                .Setup(g => g.RemoveAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            await _revokeTokenAction.Execute(parameter, authenticationHeader, null, null, CancellationToken.None)
                .ConfigureAwait(false);

            _grantedTokenRepositoryStub.Verify(
                g => g.RemoveAccessToken(grantedToken.AccessToken, It.IsAny<CancellationToken>()));
        }
    }
}
