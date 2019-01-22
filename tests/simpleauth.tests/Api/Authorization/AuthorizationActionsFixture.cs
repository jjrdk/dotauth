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

using SimpleAuth.Services;

namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class AuthorizationActionsFixture
    {
        private const string OpenIdScope = "openid";
        private const string HttpsLocalhost = "https://localhost";
        private Mock<IEventPublisher> _eventPublisherStub;
        private AuthorizationActions _authorizationActions;
        private Mock<IClientStore> _clientStore;

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

        private void InitializeFakeObjects(Client client = null)
        {
            _eventPublisherStub = new Mock<IEventPublisher>();
            _clientStore = new Mock<IClientStore>();
            if (client != null)
            {
                _clientStore.Setup(x => x.GetById(It.IsAny<string>()))
                    .ReturnsAsync(client);
            }

            _authorizationActions = new AuthorizationActions(
                new Mock<IGenerateAuthorizationResponse>().Object,
                _clientStore.Object,
                new Mock<IConsentRepository>().Object,
                _eventPublisherStub.Object,
                new IAuthenticateResourceOwnerService[0]);
        }
    }
}
