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

using Microsoft.IdentityModel.Tokens;
using SimpleAuth.Shared.Repositories;
using System.Net.Http.Headers;

namespace SimpleAuth.Tests.Api.Token
{
    using Errors;
    using Exceptions;
    using Logging;
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
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class GetTokenByResourceOwnerCredentialsGrantTypeActionFixture
    {
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperFake;
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperFake;
        private Mock<IClientStore> _clientStore;
        private Mock<IJwtGenerator> _jwtGeneratorFake;
        private Mock<ITokenStore> _tokenStoreStub;
        private GetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantTypeAction;

        public GetTokenByResourceOwnerCredentialsGrantTypeActionFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_No_Request_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(new AuthenticationResult(null, "error")));
            //_clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
            //    .Returns(() => Task.FromResult((Client)null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                        resourceOwnerGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
            Assert.Equal(ErrorDescriptions.TheClientDoesntExist, exception.Message);
        }

        [Fact]
        public async Task When_Client_GrantType_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() =>
            //    {
            //        return Task.FromResult(new AuthenticationResult(client, null));
            //    });
            //_clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
            //    .Returns(() => Task.FromResult((Client)null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                        resourceOwnerGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, clientId, GrantType.password),
                exception.Message);
        }

        [Fact]
        public async Task When_Client_ResponseTypes_Are_Not_Valid_Then_Exception_Is_Thrown()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ResponseTypes = new string[0],
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.password }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(new AuthenticationResult(,
            //        null)));
            //_clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
            //    .Returns(() => Task.FromResult((Client)null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                        resourceOwnerGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, clientId, "token id_token"),
                exception.Message);
        }

        [Fact]
        public async Task When_The_Resource_Owner_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                    r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult((ResourceOwner)null));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                        resourceOwnerGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
        }

        [Fact]
        public async Task When_Passing_A_Not_Allowed_Scopes_Then_Exception_Is_Throw()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            const string invalidScope = "invalidScope";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = invalidScope
            };
            var client = new Client
            {
                ClientId = "id",
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };

            var resourceOwner = new ResourceOwner();
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                    r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult(resourceOwner));
            //_scopeValidatorFake.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Client>()))
            //    .Returns(() => new ScopeValidationResult("error"));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                        resourceOwnerGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidScope, exception.Code);
        }

        [Fact]
        public async Task When_Requesting_An_AccessToken_For_An_Authenticated_User_Then_AccessToken_Is_Granted()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            const string invalidScope = "invalidScope";
            const string accessToken = "accessToken";
            var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
            {
                ClientAssertion = clientAssertion,
                ClientAssertionType = clientAssertionType,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = invalidScope
            };
            var client = new Client
            {
                AllowedScopes = new[] { new Scope { Name = invalidScope } },
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                JsonWebKeys = "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify).ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                GrantTypes = new List<GrantType> { GrantType.password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };
            var resourceOwner = new ResourceOwner();
            var userInformationJwsPayload = new JwtPayload();
            var grantedToken = new GrantedToken { AccessToken = accessToken, IdTokenPayLoad = new JwtPayload() };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(client));
            _resourceOwnerAuthenticateHelperFake.Setup(
                    r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.FromResult(resourceOwner));
            //_scopeValidatorFake.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Client>()))
            //    .Returns(() => new ScopeValidationResult(new[] { invalidScope }));
            _jwtGeneratorFake.Setup(
                    j => j.GenerateUserInfoPayloadForScopeAsync(
                        It.IsAny<ClaimsPrincipal>(),
                        It.IsAny<AuthorizationParameter>()))
                .Returns(() => Task.FromResult(userInformationJwsPayload));
            //_grantedTokenHelperStub
            //    .Setup(
            //        g => g.GetValidGrantedTokenAsync(
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult((GrantedToken)null));
            _grantedTokenGeneratorHelperFake
                .Setup(
                    g => g.GenerateToken(
                        It.IsAny<Client>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IDictionary<string, object>>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>()))
                .Returns(Task.FromResult(grantedToken));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            await _getTokenByResourceOwnerCredentialsGrantTypeAction
                .Execute(resourceOwnerGrantTypeParameter, authenticationHeader, null, null)
                .ConfigureAwait(false);

            _tokenStoreStub.Verify(g => g.AddToken(grantedToken));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<AccessToClientGranted>()));
        }

        private void InitializeFakeObjects()
        {
            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(x => x.Publish(It.IsAny<AccessToClientGranted>())).Returns(Task.CompletedTask);
            _grantedTokenGeneratorHelperFake = new Mock<IGrantedTokenGeneratorHelper>();
            _resourceOwnerAuthenticateHelperFake = new Mock<IResourceOwnerAuthenticateHelper>();
            _clientStore = new Mock<IClientStore>();
            _jwtGeneratorFake = new Mock<IJwtGenerator>();
            _tokenStoreStub = new Mock<ITokenStore>();

            _getTokenByResourceOwnerCredentialsGrantTypeAction = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
                _grantedTokenGeneratorHelperFake.Object,
                _resourceOwnerAuthenticateHelperFake.Object,
                _clientStore.Object,
                _jwtGeneratorFake.Object,
                _tokenStoreStub.Object,
                _eventPublisher.Object);
        }
    }
}
