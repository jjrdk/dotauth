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

namespace SimpleAuth.Api.Authorization
{
    using Exceptions;
    using Parameters;
    using Results;
    using Shared.Models;
    using SimpleAuth.Common;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Events;
    using SimpleAuth.Properties;

    internal sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        private readonly ProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;

        public GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
            IClientStore clientStore,
            IConsentRepository consentRepository,
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _processAuthorizationRequest = new ProcessAuthorizationRequest(clientStore, consentRepository, jwksStore);
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientStore,
                consentRepository,
                jwksStore,
                eventPublisher);
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
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            var result = await _processAuthorizationRequest
                .Process(authorizationParameter, principal, client, issuerName, cancellationToken)
                .ConfigureAwait(false);
            if (!client.CheckGrantTypes(GrantTypes.Implicit, GrantTypes.AuthorizationCode))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        $"{GrantTypes.Implicit} and {GrantTypes.AuthorizationCode}"),
                    authorizationParameter.State);
            }

            if (result.Type == ActionResultType.RedirectToCallBackUrl)
            {
                result = await _generateAuthorizationResponse
                        .Generate(result, authorizationParameter, principal, client, issuerName, CancellationToken.None)
                        .ConfigureAwait(false);
            }

            return result;
        }
    }
}
