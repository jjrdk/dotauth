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
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;

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
            IEventPublisher eventPublisher)
        {
            _processAuthorizationRequest = new ProcessAuthorizationRequest(clientStore, consentRepository);
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientStore,
                consentRepository,
                eventPublisher);
        }

        public async Task<EndpointResult> Execute(
            AuthorizationParameter authorizationParameter,
            IPrincipal principal,
            Client client,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            var claimsPrincipal = principal as ClaimsPrincipal;

            var result = await _processAuthorizationRequest
                .Process(authorizationParameter, claimsPrincipal, client, issuerName, cancellationToken)
                .ConfigureAwait(false);
            if (!client.CheckGrantTypes(GrantTypes.Implicit, GrantTypes.AuthorizationCode))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        $"{GrantTypes.Implicit} and {GrantTypes.AuthorizationCode}"),
                    authorizationParameter.State);
            }

            if (result.Type == ActionResultType.RedirectToCallBackUrl)
            {
                if (claimsPrincipal == null)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                await _generateAuthorizationResponse
                    .Generate(result, authorizationParameter, claimsPrincipal, client, issuerName, CancellationToken.None)
                    .ConfigureAwait(false);
            }

            return result;
        }
    }
}
