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

namespace DotAuth.Api.Authorization;

using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Common;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
{
    private readonly ILogger _logger;
    private readonly ProcessAuthorizationRequest _processAuthorizationRequest;
    private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;

    public GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
        IClientStore clientStore,
        IConsentRepository consentRepository,
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _logger = logger;
        _processAuthorizationRequest = new ProcessAuthorizationRequest(
            clientStore,
            consentRepository,
            jwksStore,
            logger);
        _generateAuthorizationResponse = new GenerateAuthorizationResponse(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            clientStore,
            consentRepository,
            jwksStore,
            eventPublisher,
            logger);
    }

    public async Task<EndpointResult> Execute(
        AuthorizationParameter authorizationParameter,
        ClaimsPrincipal principal,
        Client client,
        string issuerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
        {
            _logger.LogError(
                string.Format(
                    Strings.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.NonceName));
            return EndpointResult.CreateBadRequestResult(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                    Status = HttpStatusCode.BadRequest
                });
        }

        var result = await _processAuthorizationRequest.Process(
                authorizationParameter,
                principal,
                client,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (result.Type == ActionResultType.BadRequest)
        {
            return result;
        }
        if (!client.CheckGrantTypes(GrantTypes.Implicit, GrantTypes.AuthorizationCode))
        {
            var message = string.Format(
                Strings.TheClientDoesntSupportTheGrantType,
                authorizationParameter.ClientId,
                $"{GrantTypes.Implicit} and {GrantTypes.AuthorizationCode}");
            _logger.LogError(message);
            return EndpointResult.CreateBadRequestResult(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
        }

        if (result.Type == ActionResultType.RedirectToCallBackUrl)
        {
            result = await _generateAuthorizationResponse.Generate(
                    result,
                    authorizationParameter,
                    principal,
                    client,
                    issuerName,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }

        return result;
    }
}