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

namespace DotAuth.WebSite.Authenticate;

using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class AuthenticateResourceOwnerOpenIdAction
{
    private readonly AuthenticateHelper _authenticateHelper;

    public AuthenticateResourceOwnerOpenIdAction(
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IConsentRepository consentRepository,
        IClientStore clientStore,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _authenticateHelper = new AuthenticateHelper(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Returns an action resultKind to the controller's action.
    /// 1). Redirect to the consent screen if the user is authenticated AND the request doesn't contain a login prompt.
    /// 2). Do nothing
    /// </summary>
    /// <param name="authorizationParameter">The parameter</param>
    /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
    /// <param name="code">Encrypted parameter</param>
    /// <param name="issuerName"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>Action resultKind to the controller's action</returns>
    public async Task<EndpointResult> Execute(
        AuthorizationParameter authorizationParameter,
        ClaimsPrincipal? resourceOwnerPrincipal,
        string? code,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        var resourceOwnerIsAuthenticated = resourceOwnerPrincipal.IsAuthenticated();
        var promptParameters = authorizationParameter.Prompt.ParsePrompts();

        // 1).
        if (resourceOwnerIsAuthenticated
            && !promptParameters.Contains(PromptParameters.Login))
        {
            var subject = resourceOwnerPrincipal.GetSubject()!;
            var claims = resourceOwnerPrincipal!.Claims.ToArray();
            return await _authenticateHelper.ProcessRedirection(
                    authorizationParameter,
                    code,
                    subject,
                    claims,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        // 2).
        return EndpointResult.CreateAnEmptyActionResultWithNoEffect();
    }
}