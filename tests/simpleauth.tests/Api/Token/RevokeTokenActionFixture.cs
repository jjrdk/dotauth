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
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Controllers;
    using SimpleAuth.Repositories;
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
            _revokeTokenAction = new RevokeTokenAction(
                _clientStore.Object,
                _grantedTokenRepositoryStub.Object,
                new InMemoryJwksRepository(),
                new Mock<ILogger<TokenController>>().Object);
        }

        [Fact]
        public async Task WhenClientDoesNotExistThenErrorIsReturned()
        {
            var parameter = new RevokeTokenParameter { Token = "access_token" };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client)null);

            var (_, error) = await _revokeTokenAction.Execute(parameter, null, null, null, CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, error.Title);
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
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret } }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientid}:{clientsecret}".Base64Encode());
            var result = await _revokeTokenAction.Execute(
                        parameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal("invalid_token", result.error.Title);
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
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret } }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
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
            var grantedToken = new GrantedToken { ClientId = clientId, AccessToken = "access_token" };
            var parameter = new RevokeTokenParameter { Token = "access_token" };

            var client = new Client
            {
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _grantedTokenRepositoryStub.Setup(g => g.GetAccessToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(grantedToken);
            _grantedTokenRepositoryStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));
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
