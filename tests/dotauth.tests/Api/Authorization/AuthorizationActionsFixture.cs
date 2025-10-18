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

namespace DotAuth.Tests.Api.Authorization;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Authorization;
using DotAuth.Events;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Results;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public sealed class AuthorizationActionsFixture
{
    private const string OpenIdScope = "openid";
    private const string HttpsLocalhost = "https://localhost";
    private IEventPublisher _eventPublisherStub = null!;
    private AuthorizationActions _authorizationActions = null!;
    private IClientStore _clientStore = null!;

    [Fact]
    public async Task WhenClientRequirePkceAndNoCodeChallengeIsPassedThenAnErrorIsReturned()
    {
        const string clientId = "clientId";
        const string scope = OpenIdScope;

        var redirectUrl = new Uri(HttpsLocalhost);
        InitializeFakeObjects(
            new Client
            {
                ResponseTypes = [ResponseTypeNames.IdToken],
                ClientId = clientId,
                RequirePkce = true,
                RedirectionUrls = [redirectUrl]
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
                CancellationToken.None);
        Assert.Equal(ActionResultType.BadRequest, result.Type);
    }

    private void InitializeFakeObjects(Client? client = null)
    {
        _eventPublisherStub = Substitute.For<IEventPublisher>();
        _clientStore = Substitute.For<IClientStore>();
        if (client != null)
        {
            _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(client);
        }

        _authorizationActions = new AuthorizationActions(
            Substitute.For<IAuthorizationCodeStore>(),
            _clientStore,
            Substitute.For<ITokenStore>(),
            Substitute.For<IScopeRepository>(),
            Substitute.For<IConsentRepository>(),
            new InMemoryJwksRepository(),
            _eventPublisherStub,
            [],
            Substitute.For<ILogger>());
    }
}
