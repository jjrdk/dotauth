﻿//// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
////
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
////
////     http://www.apache.org/licenses/LICENSE-2.0
////
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using Moq;
//using SimpleIdentityServer.Core.Authenticate;
//using SimpleIdentityServer.Core.Common;
//using SimpleIdentityServer.Core.Common.Models;
//using SimpleIdentityServer.Core.Errors;
//using SimpleIdentityServer.Core.Exceptions;
//using SimpleIdentityServer.Core.Helpers;
//using SimpleIdentityServer.Core.JwtToken;
//using SimpleIdentityServer.Core.Parameters;
//using SimpleIdentityServer.Core.Validators;
//using SimpleIdentityServer.OAuth.Logging;
//using SimpleIdentityServer.Store;
//using System;
//using System.Collections.Generic;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;
//using Xunit;

//namespace SimpleIdentityServer.Core.UnitTests.Api.Token
//{
//    public sealed class GetTokenByClientCredentialsGrantTypeActionFixture
//    {
//        private Mock<IAuthenticateInstructionGenerator> _authenticateInstructionGeneratorStub;
//        private Mock<IAuthenticateClient> _authenticateClientStub;
//        private Mock<IClientValidator> _clientValidatorStub;
//        private Mock<IGrantedTokenGeneratorHelper> _grantedTokenGeneratorHelperStub;
//        private Mock<IScopeValidator> _scopeValidatorStub;
//        private Mock<IOAuthEventSource> _oauthEventSource;
//        private Mock<IClientCredentialsGrantTypeParameterValidator> _clientCredentialsGrantTypeParameterValidatorStub;
//        private Mock<IClientHelper> _clientHelperStub;
//        private Mock<IJwtGenerator> _jwtGeneratorStub;
//        private Mock<ITokenStore> _tokenStoreStub;
//        private Mock<IGrantedTokenHelper> _grantedTokenHelperStub;

//        [Fact]
//        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
//        {
////            InitializeFakeObjects();

//            //            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(null, null, null, null)).ConfigureAwait(false);
//        }

//        [Fact]
//        public async Task When_Client_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
//        {
////            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = "scope"
//            };
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .Returns(() => Task.FromResult(new AuthenticationResult(null, null)));

//            //            var exception = await Assert.ThrowsAsync<DotAuthException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null)).ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
//        }

//        [Fact]
//        public async Task When_ClientCredentialGrantType_Is_Not_Supported_Then_Exception_Is_Thrown()
//        {
////            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = "scope"
//            };
//            var client = new AuthenticationResult(new Client
//            {
//                GrantTypes = new []
//                {
//                    GrantTypes.password
//                }
//            }, null);
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .ReturnsAsync(client));

//            //            var exception = await Assert.ThrowsAsync<DotAuthException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null)).ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
//            Assert.True(exception.Message == string.Format(Strings.TheClientDoesntSupportTheGrantType, client.Client.ClientId, GrantTypes.client_credentials));
//        }

//        [Fact]
//        public async Task When_TokenResponseType_Is_Not_Supported_Then_Exception_Is_Thrown()
//        {
////            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = "scope"
//            };
//            var client = new AuthenticationResult(new Client
//            {
//                GrantTypes = new []
//                {
//                    GrantTypes.client_credentials
//                },
//                ResponseTypes = new List<ResponseType>
//                {
//                    ResponseType.code
//                }
//            }, null);
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .ReturnsAsync(client));

//            //            var exception = await Assert.ThrowsAsync<DotAuthException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null)).ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidClient, exception.Code);
//            Assert.True(exception.Message == string.Format(Strings.TheClientDoesntSupportTheResponseType, client.Client.ClientId, ResponseType.token));
//        }

//        [Fact]
//        public async Task When_Scope_Is_Not_Valid_Then_Exception_Is_Thrown()
//        {
////            var messageDescription = "message_description";
//            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = "scope"
//            };
//            var client = new AuthenticationResult(new Client
//            {
//                GrantTypes = new []
//                {
//                    GrantTypes.client_credentials
//                },
//                ResponseTypes = new List<ResponseType>
//                {
//                    ResponseType.token
//                }
//            }, null);
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .ReturnsAsync(client));
//            _clientValidatorStub.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<string[]>())).Returns(Array.Empty<string>());
//            _scopeValidatorStub.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Client>()))
//                .Returns(() => new ScopeValidationResult(false)
//                {
//                    ErrorMessage = messageDescription
//                });

//            //            var exception = await Assert.ThrowsAsync<DotAuthException>(() => _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null)).ConfigureAwait(false);
//            Assert.NotNull(exception);
//            Assert.Equal(ErrorCodes.InvalidScope, exception.Code);
//            Assert.True(exception.Message == messageDescription);
//        }

//        [Fact]
//        public async Task When_Access_Is_Granted_Then_Token_Is_Returned()
//        {
////            const string scope = "valid_scope";
//            const string clientId = "client_id";
//            const string accessToken = "access_token";
//            var scopes = new List<string> { scope };
//            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = scope
//            };
//            var client = new AuthenticationResult(new Client
//            {
//                GrantTypes = new []
//                {
//                    GrantTypes.client_credentials
//                },
//                ResponseTypes = new List<ResponseType>
//                {
//                    ResponseType.token
//                },
//                ClientId = clientId
//            }, null);
//            var grantedToken = new GrantedToken
//            {
//                ClientId = clientId,
//                AccessToken = accessToken,
//                IdTokenPayLoad = new JwtSecurityToken()
//            };
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .ReturnsAsync(client));
//            _scopeValidatorStub.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Client>()))
//                .Returns(() => new ScopeValidationResult(true)
//                {
//                    Scopes = scopes
//                });
//            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateToken(
//                    It.IsAny<Client>(),
//                    It.IsAny<string>(),
//                    It.IsAny<string>(),
//                    It.IsAny<IDictionary<string, object>>(),
//                    It.IsAny<JwtSecurityToken>(),
//                    It.IsAny<JwtSecurityToken>()))
//                .ReturnsAsync(grantedToken));

//            //            var resultKind = await _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null).ConfigureAwait(false);

//            //            _oauthEventSource.Verify(s => s.GrantAccessToClient(clientId, accessToken, scope));
//            Assert.NotNull(resultKind);
//            Assert.True(resultKind.ClientId == clientId);
//        }

//        [Fact]
//        public async Task When_Access_Is_Granted_Then_Stateless_Token_Is_Returned()
//        {
////            const string scope = "valid_scope";
//            const string clientId = "client_id";
//            const string accessToken = "access_token";
//            var jwsPayload = new JwtSecurityToken();
//            var scopes = new List<string> { scope };
//            InitializeFakeObjects();
//            var clientCredentialsGrantTypeParameter = new ClientCredentialsGrantTypeParameter
//            {
//                Scope = scope
//            };
//            var client = new AuthenticationResult(new Client
//            {
//                GrantTypes = new []
//                {
//                    GrantTypes.client_credentials
//                },
//                ResponseTypes = new List<ResponseType>
//                {
//                    ResponseType.token
//                },
//                ClientId = clientId
//            }, null);
//            var grantedToken = new GrantedToken
//            {
//                AccessToken = accessToken,
//                ClientId = clientId
//            };
//            var authenticateInstruction = new AuthenticateInstruction();
//            _authenticateInstructionGeneratorStub.Setup(a => a.GetAuthenticateInstruction(It.IsAny<AuthenticationHeaderValue>()))
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Setup(a => a.Authenticate(It.IsAny<AuthenticateInstruction>(), null))
//                .ReturnsAsync(client));
//            _scopeValidatorStub.Setup(s => s.Check(It.IsAny<string>(), It.IsAny<Client>()))
//                .Returns(() => new ScopeValidationResult(true)
//                {
//                    Scopes = scopes
//                });
//            _jwtGeneratorStub.Setup(g => g.GenerateAccessToken(
//                    It.IsAny<Client>(),
//                    It.IsAny<IEnumerable<string>>(),
//                    It.IsAny<string>(),
//                    It.IsAny<IDictionary<string, object>>()))
//                .ReturnsAsync(jwsPayload));
//            _clientHelperStub.Setup(g => g.GenerateIdToken(It.IsAny<Client>(),
//                It.IsAny<JwtSecurityToken>()))
//                .ReturnsAsync(accessToken));
//            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateToken(It.IsAny<Client>(),
//                It.IsAny<string>(),
//                It.IsAny<string>(),
//                It.IsAny<IDictionary<string, object>>(),
//                It.IsAny<JwtSecurityToken>(),
//                It.IsAny<JwtSecurityToken>())).ReturnsAsync(grantedToken));

//            //            var resultKind = await _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null).ConfigureAwait(false);

//            //            _oauthEventSource.Verify(s => s.GrantAccessToClient(clientId, accessToken, scope));
//            Assert.NotNull(resultKind);
//            Assert.True(resultKind.ClientId == clientId);
//        }

//        private void InitializeFakeObjects()
//        {
//            _authenticateInstructionGeneratorStub = new Mock<IAuthenticateInstructionGenerator>();
//            _authenticateClientStub = new Mock<IAuthenticateClient>();
//            _clientValidatorStub = new Mock<IClientValidator>();
//            _grantedTokenGeneratorHelperStub = new Mock<IGrantedTokenGeneratorHelper>();
//            _scopeValidatorStub = new Mock<IScopeValidator>();
//            _oauthEventSource = new Mock<IOAuthEventSource>();
//            _clientCredentialsGrantTypeParameterValidatorStub = new Mock<IClientCredentialsGrantTypeParameterValidator>();
//            _clientHelperStub = new Mock<IClientHelper>();
//            _jwtGeneratorStub = new Mock<IJwtGenerator>();
//            _tokenStoreStub = new Mock<ITokenStore>();
//            _grantedTokenHelperStub = new Mock<IGrantedTokenHelper>();
//            _getTokenByClientCredentialsGrantTypeAction = new GetTokenByClientCredentialsGrantTypeAction(
//                _authenticateInstructionGeneratorStub.Object,
//                _authenticateClientStub.Object,
//                _clientValidatorStub.Object,
//                _grantedTokenGeneratorHelperStub.Object,
//                _scopeValidatorStub.Object,
//                _oauthEventSource.Object,
//                _clientCredentialsGrantTypeParameterValidatorStub.Object,
//                _clientHelperStub.Object,
//                _jwtGeneratorStub.Object,
//                _tokenStoreStub.Object,
//                _grantedTokenHelperStub.Object);
//        }
//    }
//}
