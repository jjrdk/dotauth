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

namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Logging;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class AuthorizationActionsFixture
    {
        private const string OpenIdScope = "openid";
        private const string HttpsLocalhost = "https://localhost";
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IAuthorizationFlowHelper> _authorizationFlowHelperFake;
        private Mock<IEventPublisher> _eventPublisherStub;
        private Mock<IAmrHelper> _amrHelperStub;
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperStub;
        private IAuthorizationActions _authorizationActions;
        private Mock<IClientStore> _clientStore;
        private Mock<IConsentHelper> _consentHelper;

        [Fact]
        public async Task When_Client_Require_PKCE_And_NoCodeChallenge_Is_Passed_Then_Exception_Is_Thrown()
        {
            const string clientId = "clientId";
            const string scope = OpenIdScope;

            var redirectUrl = new Uri(HttpsLocalhost);
            InitializeFakeObjects(
                new Client
                {
                    ResponseTypes = new[] { ResponseTypeNames.IdToken },
                    ClientId = clientId,
                    RequirePkce = true,
                    RedirectionUrls = new[] { redirectUrl }
                });

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = ResponseTypeNames.IdToken,
                Scope = scope,
                RedirectUrl = redirectUrl
            };

            var result = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() => _authorizationActions.GetAuthorization(authorizationParameter, null, null)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, result.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheClientRequiresPkce, clientId), result.Message);
        }

        [Fact]
        public async Task When_Starting_Implicit_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            const string clientId = "clientId";
            const string responseType = ResponseTypeNames.IdToken;
            const string scope = OpenIdScope;
            const string actionType = "RedirectToAction";
            const string controllerAction = "AuthenticateIndex"; //"ConsentIndex";
            var redirectUrl = new Uri(HttpsLocalhost);

            InitializeFakeObjects(
                new Client
                {
                    ClientId = clientId,
                    RequirePkce = false,
                    ResponseTypes = new[] { responseType },
                    GrantTypes = new[] { GrantType.@implicit, GrantType.authorization_code },
                    RedirectionUrls = new[] { redirectUrl },
                    AllowedScopes = new[] { new Scope { Name = "openid" } }
                });
            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToAction,
            //    RedirectInstruction = new RedirectInstruction
            //    {
            //        Action = SimpleAuthEndPoints.ConsentIndex
            //    }
            //};

            //_authorizationCodeGrantTypeParameterAuthEdpValidatorFake.Setup(a => a.ValidateAsync(It.IsAny<AuthorizationParameter>()))
            //    .Returns(Task.FromResult(new Client
            //    {
            //        RequirePkce = false
            //    }));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new[]
                {
                    ResponseTypeNames.IdToken
                });
            //_getTokenViaImplicitWorkflowOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
            //    It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<string>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.ImplicitFlow);

            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = "nonce",
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null,
                RedirectUrl = redirectUrl
            };
            var serializedParameter = "[]"; //actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();

            var result = await _authorizationActions.GetAuthorization(authorizationParameter, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        [Fact]
        public async Task When_Starting_AuthorizationCode_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            var redirectUrl = new Uri(HttpsLocalhost);
            InitializeFakeObjects(
                new Client
                {
                    RequirePkce = false,
                    RedirectionUrls = new[] { redirectUrl, },
                    AllowedScopes = new[] { new Scope { Name = OpenIdScope } },
                    ResponseTypes = new[] { ResponseTypeNames.IdToken }
                });
            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToAction,
            //    RedirectInstruction = new RedirectInstruction
            //    {
            //        Action = SimpleAuthEndPoints.ConsentIndex
            //    }
            //};

            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<string>
                {
                    ResponseTypeNames.IdToken
                });
            _parameterParserHelperFake.Setup(x => x.ParseScopes(It.IsAny<string>())).Returns(new[] { OpenIdScope });
            //_getAuthorizationCodeOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
            //    It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<string>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.AuthorizationCodeFlow);

            const string clientId = "clientId";
            const string responseType = ResponseTypeNames.IdToken;
            const string actionType = "RedirectToAction";
            const string controllerAction = "AuthenticateIndex";// "ConsentIndex";

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                ResponseType = responseType,
                Scope = OpenIdScope,
                Claims = null,
                RedirectUrl = redirectUrl
            };
            var serializedParameter = "[]"; //actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();

            await _authorizationActions.GetAuthorization(authorizationParameter, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, OpenIdScope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        [Fact]
        public async Task When_Starting_Hybrid_Authorization_Process_Then_Event_Is_Started_And_Ended()
        {
            const string clientId = "clientId";
            const string scope = OpenIdScope;
            const string actionType = "RedirectToAction";
            const string controllerAction = "AuthenticateIndex"; //"ConsentIndex";

            var redirectUrl = new Uri(HttpsLocalhost);
            var responseType = ResponseTypeNames.IdToken;
            InitializeFakeObjects(
                new Client
                {
                    ClientId = clientId,
                    RequirePkce = false,
                    ResponseTypes = new[] { responseType },
                    GrantTypes = new[] { GrantType.@implicit, GrantType.authorization_code },
                    RedirectionUrls = new[] { redirectUrl },
                    AllowedScopes = new[] { new Scope { Name = "openid" } }
                });
            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToAction,
            //    RedirectInstruction = new RedirectInstruction
            //    {
            //        Action = SimpleAuthEndPoints.ConsentIndex
            //    }
            //};

            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<string>
                {
                    responseType
                });
            //_getAuthorizationCodeAndTokenViaHybridWorkflowOperationFake.Setup(g => g.Execute(It.IsAny<AuthorizationParameter>(),
            //    It.IsAny<IPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _authorizationFlowHelperFake.Setup(a => a.GetAuthorizationFlow(It.IsAny<ICollection<string>>(),
                It.IsAny<string>()))
                .Returns(AuthorizationFlow.HybridFlow);

            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = "nonce",
                ClientId = clientId,
                ResponseType = responseType,
                Scope = scope,
                Claims = null,
                RedirectUrl = redirectUrl
            };
            var serializedParameter = "[]"; //actionResult.RedirectInstruction.Parameters.SerializeWithJavascript();

            var result = await _authorizationActions.GetAuthorization(authorizationParameter, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartAuthorization(clientId, responseType, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorization(actionType, controllerAction, serializedParameter));
        }

        private void InitializeFakeObjects(Client client = null)
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _authorizationFlowHelperFake = new Mock<IAuthorizationFlowHelper>();
            _eventPublisherStub = new Mock<IEventPublisher>();
            _amrHelperStub = new Mock<IAmrHelper>();
            _resourceOwnerAuthenticateHelperStub = new Mock<IResourceOwnerAuthenticateHelper>();
            _clientStore = new Mock<IClientStore>();
            if (client != null)
            {
                _clientStore.Setup(x => x.GetById(It.IsAny<string>()))
                    .ReturnsAsync(client);
            }

            _consentHelper = new Mock<IConsentHelper>();
            _consentHelper
                .Setup(x => x.GetConfirmedConsentsAsync(It.IsAny<string>(), It.IsAny<AuthorizationParameter>()))
                .ReturnsAsync(new Consent { });
            _authorizationActions = new AuthorizationActions(
                _consentHelper.Object,
                new Mock<IGenerateAuthorizationResponse>().Object,
                _parameterParserHelperFake.Object,
                _clientStore.Object,
                _oauthEventSource.Object,
                _authorizationFlowHelperFake.Object,
                _eventPublisherStub.Object,
                _amrHelperStub.Object,
                _resourceOwnerAuthenticateHelperStub.Object);
        }
    }
}
