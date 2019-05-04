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

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleAuth.Shared.Repositories;
using System.Net.Http.Headers;

namespace SimpleAuth.Tests.Api.Token
{
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Shared.Errors;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Repositories;
    using SimpleAuth.Tests.Helpers;
    using Xunit;

    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private readonly Mock<ITokenStore> _tokenStoreStub;
        private readonly Mock<IClientStore> _clientStore;
        private readonly GetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        public GetTokenByRefreshTokenGrantTypeActionFixture()
        {
            IdentityModelEventSource.ShowPII = true;
            _tokenStoreStub = new Mock<ITokenStore>();
            _clientStore = new Mock<IClientStore>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                new RuntimeSettings(),
                new Mock<IEventPublisher>().Object,
                _tokenStoreStub.Object,
                new Mock<IScopeRepository>().Object,
                new InMemoryJwksRepository(),
                new InMemoryResourceOwnerRepository(),
                _clientStore.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(
                        null,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client)null);

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(
                        parameter,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, ex.Code);
            Assert.Equal(ErrorDescriptions.TheClientDoesntExist, ex.Message);
        }

        [Fact]
        public async Task When_Client_Does_Not_Support_GrantType_RefreshToken_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var client = new Client
            {
                ClientId = "id",
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(
                        parameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, ex.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, "id", GrantTypes.RefreshToken),
                ex.Message);
        }

        [Fact]
        public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var client = new Client
            {
                ClientId = "id",
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
                GrantTypes = new[] { GrantTypes.RefreshToken }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((GrantedToken)null));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(
                        parameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, ex.Code);
            Assert.Equal(ErrorDescriptions.TheRefreshTokenIsNotValid, ex.Message);
        }

        [Fact]
        public async Task When_RefreshToken_Is_Not_Issued_By_The_Same_Client_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var client = new Client
            {
                ClientId = "id",
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
                GrantTypes = new[] { GrantTypes.RefreshToken }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new GrantedToken { ClientId = "differentId" }));

            var authenticationValue = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(
                        parameter,
                        authenticationValue,
                        null,
                        "issuer",
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, ex.Code);
            Assert.Equal(ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer, ex.Message);
        }

        [Fact]
        public async Task When_Requesting_Token_Then_New_One_Is_Generated()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var grantedToken = new GrantedToken { IdTokenPayLoad = new JwtPayload(), ClientId = "id", Scope = "scope" };
            var client = new Client
            {
                ClientId = "id",
                JsonWebKeys =
                    TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
                GrantTypes = new[] { GrantTypes.RefreshToken }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(grantedToken);

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            await _getTokenByRefreshTokenGrantTypeAction.Execute(
                    parameter,
                    authenticationHeader,
                    null,
                    "issuer",
                    CancellationToken.None)
                .ConfigureAwait(false);

            _tokenStoreStub.Verify(g => g.AddToken(It.IsAny<GrantedToken>(), It.IsAny<CancellationToken>()));
        }
    }
}
