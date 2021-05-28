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
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using SimpleAuth.Events;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Services;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.OAuth;
    using SimpleAuth.Shared.Properties;
    using SimpleAuth.Tests.Helpers;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GetTokenByResourceOwnerCredentialsGrantTypeActionFixture
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IClientStore> _clientStore;
        private Mock<ITokenStore> _tokenStoreStub;
        private GetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantTypeAction;
        private readonly Mock<IScopeRepository> _scopeRepository;

        public GetTokenByResourceOwnerCredentialsGrantTypeActionFixture(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _scopeRepository = new Mock<IScopeRepository>();
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
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

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;

            Assert.Equal(ErrorCodes.InvalidClient, result.Details.Title);
            Assert.Equal(string.Format(SharedStrings.TheClientDoesntExist), result.Details.Detail);
        }

        [Fact]
        public async Task When_Client_GrantType_Is_Not_Valid_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
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
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;

            Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
            Assert.Equal(
                string.Format(Strings.TheClientDoesntSupportTheGrantType, clientId, GrantTypes.Password),
                result.Details.Detail);
        }

        [Fact]
        public async Task When_Client_ResponseTypes_Are_Not_Valid_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
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
                ResponseTypes = Array.Empty<string>(),
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.Password }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;

            Assert.Equal(ErrorCodes.InvalidResponse, result.Details.Title);
            Assert.Equal(
                string.Format(Strings.TheClientDoesntSupportTheResponseType, clientId, "token id_token"),
                result.Details.Detail);
        }

        [Fact]
        public async Task When_The_Resource_Owner_Is_Not_Valid_Then_Error_Is_Returned()
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
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.Password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };

            var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
            authenticateService.SetupGet(x => x.Amr).Returns("pwd");
            authenticateService
                .Setup(
                    x => x.AuthenticateResourceOwner(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceOwner)null);
            InitializeFakeObjects(authenticateService.Object);
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;
            Assert.Equal(ErrorCodes.InvalidCredentials, result.Details.Title);
            Assert.Equal(Strings.ResourceOwnerCredentialsAreNotValid, result.Details.Detail);
        }

        [Fact]
        public async Task When_Passing_A_Not_Allowed_Scopes_Then_Error_Is_Returned()
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
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.Password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };

            var resourceOwner = new ResourceOwner();
            var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
            authenticateService
                .Setup(
                    x => x.AuthenticateResourceOwner(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(resourceOwner);
            authenticateService.Setup(x => x.Amr).Returns("pwd");
            InitializeFakeObjects(authenticateService.Object);
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<GrantedToken>.Error;

            Assert.Equal(ErrorCodes.InvalidScope, result.Details.Title);
        }

        [Fact]
        public async Task When_Requesting_An_AccessToken_For_An_Authenticated_User_Then_AccessToken_Is_Granted()
        {
            const string clientAssertion = "clientAssertion";
            const string clientAssertionType = "clientAssertionType";
            const string clientId = "clientId";
            const string clientSecret = "clientSecret";
            const string invalidScope = "invalidScope";
            //const string accessToken = "accessToken";
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
                AllowedScopes = new[] { invalidScope },
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                JsonWebKeys =
                    TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                GrantTypes = new[] { GrantTypes.Password },
                ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
            };
            var resourceOwner = new ResourceOwner { Subject = "tester" };
            var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
            authenticateService
                .Setup(
                    x => x.AuthenticateResourceOwner(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(resourceOwner);
            authenticateService.SetupGet(x => x.Amr).Returns("pwd");
            InitializeFakeObjects(authenticateService.Object);
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _scopeRepository.Setup(x => x.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(new[] { new Scope { Name = invalidScope } });

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "issuer",
                    CancellationToken.None)
                .ConfigureAwait(false);

            _tokenStoreStub.Verify(g => g.AddToken(It.IsAny<GrantedToken>(), It.IsAny<CancellationToken>()));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<TokenGranted>()));
        }

        private void InitializeFakeObjects(params IAuthenticateResourceOwnerService[] services)
        {
            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(x => x.Publish(It.IsAny<TokenGranted>())).Returns(Task.CompletedTask);
            _clientStore = new Mock<IClientStore>();
            _tokenStoreStub = new Mock<ITokenStore>();

            _getTokenByResourceOwnerCredentialsGrantTypeAction = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
                _clientStore.Object,
                _scopeRepository.Object,
                _tokenStoreStub.Object,
                new InMemoryJwksRepository(),
                services,
                _eventPublisher.Object,
                new TestOutputLogger("test", _outputHelper));
        }
    }
}
