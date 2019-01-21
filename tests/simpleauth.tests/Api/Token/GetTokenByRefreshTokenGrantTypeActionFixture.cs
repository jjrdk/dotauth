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

using SimpleAuth.Shared.Repositories;
using System.Net.Http.Headers;

namespace SimpleAuth.Tests.Api.Token
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class GetTokenByRefreshTokenGrantTypeActionFixture
    {
        private Mock<IClientHelper> _clientHelperFake;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;
        private Mock<ITokenStore> _tokenStoreStub;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private Mock<IClientStore> _clientStore;
        private GetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        public GetTokenByRefreshTokenGrantTypeActionFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync((Client) null);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(new AuthenticationResult(null, "error")));

            var ex = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, null, null, null))
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
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                GrantTypes = new List<GrantType> {GrantType.authorization_code}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(new AuthenticationResult(,
            //        null)));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, authenticationHeader, null, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, ex.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, "id", GrantType.refresh_token),
                ex.Message);
        }

        [Fact]
        public async Task When_Passing_Invalid_Refresh_Token_Then_Exception_Is_Thrown()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var client = new Client
            {
                ClientId = "id",
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                GrantTypes = new List<GrantType> {GrantType.refresh_token}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(new AuthenticationResult(,
            //        null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult((GrantedToken) null));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, authenticationHeader, null, null))
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
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                GrantTypes = new List<GrantType> {GrantType.refresh_token}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(new AuthenticationResult(,
            //        null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new GrantedToken {ClientId = "differentId"}));

            var authenticationValue = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            var ex = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, authenticationValue, null, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, ex.Code);
            Assert.Equal(ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer, ex.Message);
        }

        [Fact]
        public async Task When_Requesting_Token_Then_New_One_Is_Generated()
        {
            var parameter = new RefreshTokenGrantTypeParameter();
            var grantedToken = new GrantedToken {IdTokenPayLoad = new JwtPayload(), ClientId = "id"};
            var client = new Client
            {
                ClientId = "id",
                Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                GrantTypes = new List<GrantType> {GrantType.refresh_token}
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(new AuthenticationResult(,
            //        null)));
            _tokenStoreStub.Setup(g => g.GetRefreshToken(It.IsAny<string>())).Returns(Task.FromResult(grantedToken));
            _grantedTokenGeneratorHelperStub
                .Setup(
                    g => g.GenerateToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IDictionary<string, object>>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>()))
                .Returns(Task.FromResult(grantedToken));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", "id:secret".Base64Encode());
            await _getTokenByRefreshTokenGrantTypeAction.Execute(parameter, authenticationHeader, null, null)
                .ConfigureAwait(false);

            _tokenStoreStub.Verify(g => g.AddToken(It.IsAny<GrantedToken>()));
        }

        private void InitializeFakeObjects()
        {
            _clientHelperFake = new Mock<IClientHelper>();
            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
            _tokenStoreStub = new Mock<ITokenStore>();
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _clientStore = new Mock<IClientStore>();
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                _clientHelperFake.Object,
                new Mock<IEventPublisher>().Object,
                _grantedTokenGeneratorHelperStub.Object,
                _tokenStoreStub.Object,
                _jwtGeneratorStub.Object,
                _clientStore.Object);
        }
    }
}
