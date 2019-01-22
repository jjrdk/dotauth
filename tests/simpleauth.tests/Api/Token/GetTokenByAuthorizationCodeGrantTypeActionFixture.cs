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
    using Logging;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.JwtToken;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class GetTokenByAuthorizationCodeGrantTypeActionFixture
    {
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IAuthorizationCodeStore> _authorizationCodeStoreFake;
        private OAuthConfigurationOptions _oauthConfigurationOptions;
        private Mock<ITokenStore> _tokenStoreFake;
        private Mock<IClientStore> _clientStore;
        private Mock<IJwtGenerator> _jwtGeneratorStub;
        private GetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;

        [Fact]
        public async Task When_Passing_Empty_Request_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = "clientId",
                ClientSecret = "clientSecret"
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync((Client)null);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(new AuthenticationResult(null, null)));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        null,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
        }

        [Fact]
        public async Task When_Client_Does_Not_Support_Grant_Type_Code_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
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
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
            };
            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(new AuthenticationResult(,
            //        null)));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, clientId, GrantType.authorization_code),
                exception.Message);
        }

        [Fact]
        public async Task When_Client_Does_Not_Support_ResponseType_Code_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            var client = new Client
            {
                ResponseTypes = new string[0],
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(new AuthenticationResult(,
            //        null)));
            var authenticationValue = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationValue,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, clientId, ResponseTypeNames.Code),
                exception.Message);
        }

        [Fact]
        public async Task When_Authorization_Code_Is_Not_Valid_Then_Exception_Is_Thrown()
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
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);

            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(client));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => Task.FromResult((AuthorizationCode)null));
            var authorizationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authorizationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationCodeIsNotCorrect, exception.Message);
        }

        [Fact]
        public async Task When_Pkce_Validation_Failed_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
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
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);

            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .ReturnsAsync(client);
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => Task.FromResult(authorizationCode));
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(false);

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(ErrorDescriptions.TheCodeVerifierIsNotCorrect, exception.Message);
        }

        [Fact]
        public async Task When_Granted_Client_Is_Not_The_Same_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var clientSecret = "clientSecret";
            var clientId = "notCorrectClientId";
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
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode { ClientId = "clientId" };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(() => Task.FromResult(result));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(() => Task.FromResult(authorizationCode));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                    authorizationCodeGrantTypeParameter.ClientId),
                exception.Message);
        }

        [Fact]
        public async Task When_Redirect_Uri_Is_Not_The_Same_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://notCorrectRedirectUri")
            };

            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri")
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(result));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(authorizationCode));
            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(ErrorDescriptions.TheRedirectionUrlIsNotTheSame, exception.Message);
        }

        [Fact]
        public async Task When_The_Authorization_Code_Has_Expired_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(2));
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId,
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTime.UtcNow.AddSeconds(-30)
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(result));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(authorizationCode));

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationCodeIsObsolete, exception.Message);
        }

        [Fact]
        public async Task When_RedirectUri_Is_Different_From_The_One_Hold_By_The_Client_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(3000));
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientAssertion = "clientAssertion",
                ClientAssertionType = "clientAssertionType",
                ClientSecret = clientSecret,
                RedirectUri = new Uri("https://redirectUri"),
                ClientId = clientId,
            };
            var client = new Client
            {
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code }
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTime.UtcNow
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            _authorizationCodeStoreFake.Setup(a => a.RemoveAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(result));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(authorizationCode));
            //_clientValidatorFake.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<Uri[]>()))
            //    .Returns(new Uri[0]);

            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                        authorizationCodeGrantTypeParameter,
                        authenticationHeader,
                        null,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidGrant, exception.Code);
            Assert.Equal(
                string.Format(ErrorDescriptions.RedirectUrlIsNotValid, "https://redirecturi/"),
                exception.Message);
        }

        [Fact]
        public async Task When_Requesting_An_Existed_Granted_Token_Then_Check_Id_Token_Is_Signed_And_Encrypted()
        {
            InitializeFakeObjects(TimeSpan.FromSeconds(3000));
            const string accessToken = "accessToken";
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
                AllowedScopes = new List<Scope> { new Scope { Name = "scope" } },
                RedirectionUrls = new[] { new Uri("https://redirectUri") },
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code },
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
            };
            var authorizationCode = new AuthorizationCode
            {
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTime.UtcNow,
                Scopes = "scope"
            };
            var grantedToken = new GrantedToken
            {
                ClientId = clientId,
                AccessToken = accessToken,
                IdToken = identityToken,
                IdTokenPayLoad = new JwtPayload(),
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = 100000
            };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(result));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(authorizationCode));
            //_clientValidatorFake.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<Uri>()))
            //    .Returns(new[] { new Uri("https://redirectUri") });
            //_grantedTokenHelperStub
            //    .Setup(
            //        g => g.GetValidGrantedTokenAsync(
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult(grantedToken));
            _tokenStoreFake
                .Setup(
                    x => x.GetToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>()))
                .ReturnsAsync(grantedToken);
            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var r = await _getTokenByAuthorizationCodeGrantTypeAction
                .Execute(authorizationCodeGrantTypeParameter, authenticationHeader, null, null)
                .ConfigureAwait(false);

            Assert.NotNull(r);
        }

        [Fact]
        public async Task When_Requesting_Token_And_There_Is_No_Valid_Granted_Token_Then_Grant_A_New_One()
        {
            InitializeFakeObjects();
            //const string accessToken = "accessToken";
            //const string identityToken = "identityToken";
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
                RedirectionUrls = new[] { new Uri("https://redirectUri") },
                ClientId = clientId,
                Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
                GrantTypes = new List<GrantType> { GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.Code },
                JsonWebKeys = "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify).ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
            };
            var authorizationCode = new AuthorizationCode
            {
                Scopes = "scope",
                ClientId = clientId,
                RedirectUri = new Uri("https://redirectUri"),
                CreateDateTime = DateTime.UtcNow
            };
            //var grantedToken = new GrantedToken { AccessToken = accessToken, IdToken = identityToken };

            _clientStore.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(client);
            //_clientValidatorFake.Setup(c =>
            //        c.CheckPkce(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<AuthorizationCode>()))
            //    .Returns(true);
            //_clientStore.Setup(a => a.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), null))
            //    .Returns(Task.FromResult(authResult));
            _authorizationCodeStoreFake.Setup(a => a.GetAuthorizationCode(It.IsAny<string>()))
                .Returns(Task.FromResult(authorizationCode));
            _oauthConfigurationOptions =
                new OAuthConfigurationOptions(authorizationCodeValidity: TimeSpan.FromSeconds(3000));
            //.Setup(s => s.GetAuthorizationCodeValidityPeriodInSecondsAsync())
            //.Returns(Task.FromResult((double)3000));
            //_clientValidatorFake.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<Uri>()))
            //    .Returns(new[] { new Uri("https://redirectUri") });
            _tokenStoreFake.Setup(
                    x => x.GetToken(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<JwtPayload>(),
                        It.IsAny<JwtPayload>()))
                .ReturnsAsync((GrantedToken)null);
            //_grantedTokenGeneratorHelperFake
            //    .Setup(
            //        g => g.GenerateToken(
            //            It.IsAny<Client>(),
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<IDictionary<string, object>>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(Task.FromResult(grantedToken));
            //_grantedTokenHelperStub
            //    .Setup(
            //        g => g.GetValidGrantedTokenAsync(
            //            It.IsAny<string>(),
            //            It.IsAny<string>(),
            //            It.IsAny<JwtPayload>(),
            //            It.IsAny<JwtPayload>()))
            //    .Returns(() => Task.FromResult((GrantedToken)null));

            var authenticationHeader = new AuthenticationHeaderValue("Basic", $"{clientId}:{clientSecret}".Base64Encode());
            var result = await _getTokenByAuthorizationCodeGrantTypeAction
                .Execute(authorizationCodeGrantTypeParameter, authenticationHeader, null, null)
                .ConfigureAwait(false);

            _tokenStoreFake.Verify(g => g.AddToken(It.IsAny<GrantedToken>()));
            _eventPublisher.Verify(s => s.Publish(It.IsAny<AccessToClientGranted>()));
        }

        private void InitializeFakeObjects(TimeSpan authorizationCodeValidity = default)
        {
            _eventPublisher = new Mock<IEventPublisher>();
            _authorizationCodeStoreFake = new Mock<IAuthorizationCodeStore>();
            _tokenStoreFake = new Mock<ITokenStore>();
            _clientStore = new Mock<IClientStore>();
            _oauthConfigurationOptions = new OAuthConfigurationOptions(
                authorizationCodeValidity: authorizationCodeValidity == default
                    ? TimeSpan.FromSeconds(3600)
                    : authorizationCodeValidity);
            _jwtGeneratorStub = new Mock<IJwtGenerator>();
            _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
                _authorizationCodeStoreFake.Object,
                _oauthConfigurationOptions,
                _clientStore.Object,
                _eventPublisher.Object,
                _tokenStoreFake.Object,
                _jwtGeneratorStub.Object);
        }
    }
}
