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
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Logging;
    using SimpleAuth.Events;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Events.OAuth;
    using Xunit;

    public sealed class GetTokenByAuthorizationCodeGrantTypeActionFixture
    {
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IAuthorizationCodeStore> _authorizationCodeStoreFake;
        private RuntimeSettings _simpleAuthOptions;
        private Mock<ITokenStore> _tokenStoreFake;
        private Mock<IClientStore> _clientStore;
        private GetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
        private InMemoryJwksRepository _inMemoryJwksRepository;

        public GetTokenByAuthorizationCodeGrantTypeActionFixture()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "clientId",
                ClientSecret = "clientSecret"
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client)null);

            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                    authorizationCodeGrantTypeParameter,
                    null,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, result.Error.Title);
        }

        [Fact]
        public async Task When_Client_Does_Not_Support_Grant_Type_Code_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, result.Error.Title);
            Assert.Equal(
                string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    clientId,
                    GrantTypes.AuthorizationCode),
                result.Error.Detail);
        }

        [Fact]
        public async Task When_Client_Does_Not_Support_ResponseType_Code_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ResponseTypes = Array.Empty<string>(),
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            var authenticationValue = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationValue,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidResponse, result.Error.Title);
            Assert.Equal(
                string.Format(
                    Strings.TheClientDoesntSupportTheResponseType,
                    clientId,
                    ResponseTypeNames.Code),
                result.Error.Detail);
        }

        [Fact]
        public async Task When_Authorization_Code_Is_Not_Valid_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientSecret = "clientSecret";
            var clientId = "id";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((AuthorizationCode)null));
            var authorizationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authorizationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidGrant, result.Error.Title);
            Assert.Equal(Strings.TheAuthorizationCodeIsNotCorrect, result.Error.Detail);
        }

        [Fact]
        public async Task When_Pkce_Validation_Failed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "xyz",
                CodeVerifier = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var authorizationCode = new AuthorizationCode { ClientId = clientId };
            var client = new Client
            {
                RequirePkce = true,
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(authorizationCode));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title);
            Assert.Equal(Strings.TheCodeVerifierIsNotCorrect, result.Error.Detail);
        }

        [Fact]
        public async Task When_Granted_Client_Is_Not_The_Same_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientSecret = "clientSecret";
            var clientId = "notCorrectClientId";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var client = new Client
            {
                RequirePkce = false,
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode { ClientId = "clientId" };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(authorizationCode));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title);
            Assert.Equal(
                string.Format(
                    Strings.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                    "clientId"),
                result.Error.Detail);
        }

        [Fact]
        public async Task When_Redirect_Uri_Is_Not_The_Same_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://notCorrectRedirectUri")
            };

            var client = new Client
            {
                RequirePkce = false,
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri")
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationCode);
            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Error.Title);
            Assert.Equal(Strings.TheRedirectionUrlIsNotTheSame, result.Error.Detail);
        }

        [Fact]
        public async Task When_The_Authorization_Code_Has_Expired_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(2));
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId,
            };
            var client = new Client
            {
                RequirePkce = false,
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTimeOffset.UtcNow.AddSeconds(-30)
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationCode);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.ExpiredAuthorizationCode, result.Error.Title);
            Assert.Equal(Strings.TheAuthorizationCodeIsObsolete, result.Error.Detail);
        }

        [Fact]
        public async Task When_RedirectUri_Is_Different_From_The_One_Hold_By_The_Client_Then_Error_Is_Returned()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(3000));
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId,
            };
            var client = new Client
            {
                RequirePkce = false,
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);
            _authorizationCodeStoreFake.Setup(a => a.Remove(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationCode);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null,
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Error.Title);
            Assert.Equal(
                string.Format(Strings.RedirectUrlIsNotValid, "https://redirecturi/"),
                result.Error.Detail);
        }

        [Fact]
        public async Task When_Requesting_An_Existed_Granted_Token_Then_Check_Id_Token_Is_Signed_And_Encrypted()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(3000));
            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.CreateEncodedJwt(
                "test",
                "test",
                new ClaimsIdentity(),
                null,
                null,
                DateTime.Now,
                await _inMemoryJwksRepository.GetDefaultSigningKey().ConfigureAwait(false));
            const string identityToken = "identityToken";
            const string clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId
            };

            var client = new Client
            {
                AllowedScopes = new[] { "scope" },
                RedirectionUrls = new[] { new Uri("https://redirectUri") },
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code },
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTimeOffset.UtcNow,
                Scopes = "scope"
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                AccessToken = accessToken,
                IdToken = identityToken,
                IdTokenPayLoad = new JwtPayload(),
                CreateDateTime = DateTimeOffset.UtcNow,
                ExpiresIn = 100000
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationCode);

            _tokenStoreFake
                .Setup(
                    x => x.GetToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(grantedToken);
            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var r = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                    authorizationCodeGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(r);
        }

        [Fact]
        public async Task When_Requesting_Token_And_There_Is_No_Valid_Granted_Token_Then_Grant_A_New_One()
        {
            InitializeFakeObjects();

            const string clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                Code = "abc",
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId
            };
            var client = new Client
            {
                RequirePkce = false,
                ClientName = clientId,
                RedirectionUrls = new[] { new Uri("https://redirectUri") },
                ClientId = clientId,
                Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new[] { GrantTypes.AuthorizationCode },
                ResponseTypes = new[] { ResponseTypeNames.Code },
                JsonWebKeys =
                    "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
            };
            var authorizationCode = new AuthorizationCode
            {
                Scopes = "scope",
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTimeOffset.UtcNow
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(client);

            _authorizationCodeStoreFake.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizationCode);
            _simpleAuthOptions = new RuntimeSettings(authorizationCodeValidityPeriod: TimeSpan.FromSeconds(3000));

            _tokenStoreFake.Setup(
                    x => x.GetToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((GrantedToken)null);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                    authorizationCodeGrantTypeParameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            _tokenStoreFake.Verify(g => g.AddToken(It.IsAny<GrantedToken>(), It.IsAny<CancellationToken>()));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<TokenGranted>()));
        }

        private void InitializeFakeObjects(TimeSpan authorizationCodeValidity = default)
        {
            _eventPublisher = new Mock<IEventPublisher>();
            _authorizationCodeStoreFake = new Mock<IAuthorizationCodeStore>();
            _tokenStoreFake = new Mock<ITokenStore>();
            _clientStore = new Mock<IClientStore>();
            _simpleAuthOptions = new RuntimeSettings(
                authorizationCodeValidityPeriod: authorizationCodeValidity == default
                    ? TimeSpan.FromSeconds(3600)
                    : authorizationCodeValidity);
            _inMemoryJwksRepository = new InMemoryJwksRepository();
            _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
                _authorizationCodeStoreFake.Object,
                _simpleAuthOptions,
                _clientStore.Object,
                _eventPublisher.Object,
                _tokenStoreFake.Object,
                _inMemoryJwksRepository);
        }
    }
}
