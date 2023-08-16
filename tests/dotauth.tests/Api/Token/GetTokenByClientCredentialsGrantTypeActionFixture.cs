//// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
//        private IAuthenticateInstructionGenerator _authenticateInstructionGeneratorStub;
//        private IAuthenticateClient _authenticateClientStub;
//        private IClientValidator _clientValidatorStub;
//        private IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelperStub;
//        private IScopeValidator _scopeValidatorStub;
//        private IOAuthEventSource _oauthEventSource;
//        private IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidatorStub;
//        private IClientHelper _clientHelperStub;
//        private IJwtGenerator _jwtGeneratorStub;
//        private ITokenStore _tokenStoreStub;
//        private IGrantedTokenHelper _grantedTokenHelperStub;

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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(Task.FromResult(new AuthenticationResult(null, null)));

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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(client));

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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(client));

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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(client));
//            _clientValidatorStub.GetRedirectionUrls(Arg.Any<Client>(), Arg.Any<string[]>())).Returns(Array.Empty<string>();
//            _scopeValidatorStub.Check(Arg.Any<string>(), Arg.Any<Client>())
//                .Returns(new ScopeValidationResult(false)
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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(client));
//            _scopeValidatorStub.Check(Arg.Any<string>(), Arg.Any<Client>())
//                .Returns(new ScopeValidationResult(true)
//                {
//                    Scopes = scopes
//                });
//            _grantedTokenGeneratorHelperStub.Setup(g => g.GenerateToken(
//                    Arg.Any<Client>(),
//                    Arg.Any<string>(),
//                    Arg.Any<string>(),
//                    Arg.Any<IDictionary<string, object>>(),
//                    Arg.Any<JwtSecurityToken>(),
//                    Arg.Any<JwtSecurityToken>()))
//                .Returns(grantedToken));

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
//            _authenticateInstructionGeneratorStub.GetAuthenticateInstruction(Arg.Any<AuthenticationHeaderValue>())
//                .Returns(authenticateInstruction);
//            _authenticateClientStub.Authenticate(Arg.Any<AuthenticateInstruction>(), null)
//                .Returns(client));
//            _scopeValidatorStub.Check(Arg.Any<string>(), Arg.Any<Client>())
//                .Returns(new ScopeValidationResult(true)
//                {
//                    Scopes = scopes
//                });
//            _jwtGeneratorStub.Setup(g => g.GenerateAccessToken(
//                    Arg.Any<Client>(),
//                    Arg.Any<IEnumerable<string>>(),
//                    Arg.Any<string>(),
//                    Arg.Any<IDictionary<string, object>>()))
//                .Returns(jwsPayload));
//            _clientHelperStub.GenerateIdToken(Arg.Any<Client>(,
//                Arg.Any<JwtSecurityToken>()))
//                .Returns(accessToken));
//            _grantedTokenGeneratorHelperStub.GenerateToken(Arg.Any<Client>(,
//                Arg.Any<string>(),
//                Arg.Any<string>(),
//                Arg.Any<IDictionary<string, object>>(),
//                Arg.Any<JwtSecurityToken>(),
//                Arg.Any<JwtSecurityToken>())).Returns(grantedToken));

//            //            var resultKind = await _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, null, null, null).ConfigureAwait(false);

//            //            _oauthEventSource.Verify(s => s.GrantAccessToClient(clientId, accessToken, scope));
//            Assert.NotNull(resultKind);
//            Assert.True(resultKind.ClientId == clientId);
//        }

//        private void InitializeFakeObjects()
//        {
//            _authenticateInstructionGeneratorStub = Substitute.For<IAuthenticateInstructionGenerator>();
//            _authenticateClientStub = Substitute.For<IAuthenticateClient>();
//            _clientValidatorStub = Substitute.For<IClientValidator>();
//            _grantedTokenGeneratorHelperStub = Substitute.For<IGrantedTokenGeneratorHelper>();
//            _scopeValidatorStub = Substitute.For<IScopeValidator>();
//            _oauthEventSource = Substitute.For<IOAuthEventSource>();
//            _clientCredentialsGrantTypeParameterValidatorStub = Substitute.For<IClientCredentialsGrantTypeParameterValidator>();
//            _clientHelperStub = Substitute.For<IClientHelper>();
//            _jwtGeneratorStub = Substitute.For<IJwtGenerator>();
//            _tokenStoreStub = Substitute.For<ITokenStore>();
//            _grantedTokenHelperStub = Substitute.For<IGrantedTokenHelper>();
//            _getTokenByClientCredentialsGrantTypeAction = new GetTokenByClientCredentialsGrantTypeAction(
//                _authenticateInstructionGeneratorStub,
//                _authenticateClientStub,
//                _clientValidatorStub,
//                _grantedTokenGeneratorHelperStub,
//                _scopeValidatorStub,
//                _oauthEventSource,
//                _clientCredentialsGrantTypeParameterValidatorStub,
//                _clientHelperStub,
//                _jwtGeneratorStub,
//                _tokenStoreStub,
//                _grantedTokenHelperStub);
//        }
//    }
//}
