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

namespace SimpleAuth.Tests.Common
{
    using Moq;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Common;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Logging;
    using Xunit;

    public sealed class GenerateAuthorizationResponseFixture
    {
        private readonly Mock<IAuthorizationCodeStore> _authorizationCodeRepositoryFake;
        private readonly Mock<ITokenStore> _tokenStore;
        private readonly Mock<IEventPublisher> _eventPublisher;
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly Mock<IClientStore> _clientStore;
        private readonly Mock<IConsentRepository> _consentRepository;

        public GenerateAuthorizationResponseFixture()
        {
            _authorizationCodeRepositoryFake = new Mock<IAuthorizationCodeStore>();
            _tokenStore = new Mock<ITokenStore>();
            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(x => x.Publish(It.IsAny<AuthorizationCodeGranted>())).Returns(Task.CompletedTask);
            _eventPublisher.Setup(x => x.Publish(It.IsAny<AccessToClientGranted>())).Returns(Task.CompletedTask);
            _clientStore = new Mock<IClientStore>();
            _clientStore.Setup(x => x.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Client>());

            _consentRepository = new Mock<IConsentRepository>();
            var scopeRepository = new Mock<IScopeRepository>();
            scopeRepository.Setup(x => x.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(new[] { new Scope { Name = "openid" } });
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                _authorizationCodeRepositoryFake.Object,
                _tokenStore.Object,
                scopeRepository.Object,
                _clientStore.Object,
                _consentRepository.Object,
                _eventPublisher.Object);
        }

        [Fact]
        public async Task When_There_Is_No_Logged_User_Then_Exception_Is_Throw()
        {
            var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(
                        redirectInstruction,
                        new AuthorizationParameter(),
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Client_Is_Passed_Then_Exception_Is_Thrown()
        {
            var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _generateAuthorizationResponse.Generate(
                        redirectInstruction,
                        new AuthorizationParameter(),
                        claimsPrincipal,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            // const string idToken = "idToken";
            var clientId = "client";
            var authorizationParameter =
                new AuthorizationParameter { ResponseType = ResponseTypeNames.IdToken, ClientId = clientId };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys =
                    "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames._idTokenName);
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_No_Granted_Token_Then_Token_Is_Generated_And_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                ClientId = clientId,
                Scope = scope
            };

            var client = new Client
            {
                ClientId = clientId,
                JsonWebKeys =
                    "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
            };

            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    "issuer",
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames._accessTokenName);
            _tokenStore.Verify(g => g.AddToken(It.IsAny<GrantedToken>(), It.IsAny<CancellationToken>()));
            _eventPublisher.Verify(e => e.Publish(It.IsAny<AccessToClientGranted>()));
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_A_GrantedToken_Then_Token_Is_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                ClientId = clientId,
                Scope = scope
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = Id.Create(),
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 10000
            };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            _tokenStore.Setup(
                    x => x.GetToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(grantedToken);

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client { ClientId = "client" },
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames._accessTokenName);
            Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Value == grantedToken.AccessToken);
        }

        [Fact]
        public async Task
            When_Generating_AuthorizationResponse_With_AuthorizationCode_Then_Code_Is_Added_To_The_Parameters()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "openid";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "test"), }, "fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                ClientId = clientId,
                Scope = scope
            };

            var consent = new Consent
            {
                GrantedScopes = new List<Scope> { new Scope { Name = scope } },
                Client = new Client { ClientId = clientId, AllowedScopes = new List<Scope> { new Scope { Name = scope } } }
            };
            var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { consent });

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client(),
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(
                actionResult.RedirectInstruction.Parameters,
                p => p.Name == CoreConstants.StandardAuthorizationResponseNames._authorizationCodeName);
            _authorizationCodeRepositoryFake.Verify(a => a.AddAuthorizationCode(It.IsAny<AuthorizationCode>()));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<AuthorizationCodeGranted>()));
        }

        [Fact]
        public async Task
            When_Redirecting_To_Callback_And_There_Is_No_Response_Mode_Specified_Then_The_Response_Mode_Is_Set()
        {
            //const string idToken = "idToken";
            const string clientId = "clientId";
            const string scope = "scope";
            const string responseType = "id_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                ResponseType = responseType,
                ResponseMode = ResponseModes.None
            };

            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = ActionResultType.RedirectToCallBackUrl
            };

            await _generateAuthorizationResponse.Generate(
                    actionResult,
                    authorizationParameter,
                    claimsPrincipal,
                    new Client(),
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(ResponseModes.Fragment, actionResult.RedirectInstruction.ResponseMode);
        }
    }
}
