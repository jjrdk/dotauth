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

namespace SimpleAuth.Api.Authorization;

using System.Net;
using Parameters;
using Results;
using Shared.Models;
using SimpleAuth.Common;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Repositories;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Events;
using SimpleAuth.Extensions;
using SimpleAuth.Properties;
using SimpleAuth.Shared.Errors;

internal sealed class GetAuthorizationCodeOperation
{
    private const string AuthorizationCode = "authorization_code";
    private readonly ILogger _logger;
    private readonly ProcessAuthorizationRequest _processAuthorizationRequest;
    private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;

    public GetAuthorizationCodeOperation(
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IClientStore clientStore,
        IConsentRepository consentRepository,
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
        // 1. Check the client is authorized to use the authorization_code flow.
        if (!client.CheckGrantTypes(GrantTypes.AuthorizationCode))
        {
            _logger.LogError(
                string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    authorizationParameter.ClientId,
                    AuthorizationCode));
            return EndpointResult.CreateBadRequestResult(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = string.Format(
                        Strings.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        AuthorizationCode),
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