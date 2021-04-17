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
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Authorization;
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Events;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Results;
    using SimpleAuth.Services;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public sealed class AuthorizationActionsFixture
    {
        private const string OpenIdScope = "openid";
        private const string HttpsLocalhost = "https://localhost";
        private Mock<IEventPublisher> _eventPublisherStub;
        private AuthorizationActions _authorizationActions;
        private Mock<IClientStore> _clientStore;

        [Fact]
        public async Task WhenClientRequirePKCEAndNoCodeChallengeIsPassedThenAnErrorIsReturned()
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

            var result = await _authorizationActions.GetAuthorization(
                        authorizationParameter,
                        new ClaimsPrincipal(),
                        "",
                        CancellationToken.None)
                .ConfigureAwait(false);
            Assert.Equal(ActionResultType.BadRequest, result.Type);
        }

        private void InitializeFakeObjects(Client client = null)
        {
            _eventPublisherStub = new Mock<IEventPublisher>();
            _clientStore = new Mock<IClientStore>();
            if (client != null)
            {
                _clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(client);
            }

            _authorizationActions = new AuthorizationActions(
                new Mock<IAuthorizationCodeStore>().Object,
                _clientStore.Object,
                new Mock<ITokenStore>().Object,
                new Mock<IScopeRepository>().Object,
                new Mock<IConsentRepository>().Object,
                new InMemoryJwksRepository(),
                _eventPublisherStub.Object,
                Array.Empty<IAuthenticateResourceOwnerService>(),
                new Mock<ILogger>().Object);
        }
    }
}
