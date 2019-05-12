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
    using Shared;
    using Shared.Events.OAuth;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;

    internal class AuthorizationActions
    {
        private readonly GetAuthorizationCodeOperation _getAuthorizationCodeOperation;
        private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        private readonly GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

        private readonly AuthorizationCodeGrantTypeParameterAuthEdpValidator
            _authorizationCodeGrantTypeParameterValidator;

        private readonly IEventPublisher _eventPublisher;
        private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;

        public AuthorizationActions(
            IAuthorizationCodeStore authorizationCodeStore,
            IClientStore clientStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IConsentRepository consentRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher,
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices)
        {
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientStore,
                consentRepository,
                jwksStore,
                eventPublisher);
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                clientStore,
                consentRepository,
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                jwksStore,
                eventPublisher);
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                    clientStore,
                    consentRepository,
                    authorizationCodeStore,
                    tokenStore,
                    scopeRepository,
                    jwksStore,
                    eventPublisher);
            _authorizationCodeGrantTypeParameterValidator =
                new AuthorizationCodeGrantTypeParameterAuthEdpValidator(clientStore);
            _eventPublisher = eventPublisher;
            _resourceOwnerServices = resourceOwnerServices.ToArray();
        }

        public async Task<EndpointResult> GetAuthorization(
            AuthorizationParameter parameter,
            IPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var processId = Id.Create();

            var client = await _authorizationCodeGrantTypeParameterValidator.Validate(parameter, cancellationToken)
                .ConfigureAwait(false);
            EndpointResult endpointResult = null;

            if (client.RequirePkce
                && (string.IsNullOrWhiteSpace(parameter.CodeChallenge) || parameter.CodeChallengeMethod == null))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientRequiresPkce, parameter.ClientId),
                    parameter.State);
            }

            var responseTypes = parameter.ResponseType.ParseResponseTypes();
            var authorizationFlow = responseTypes.GetAuthorizationFlow(parameter.State);
            switch (authorizationFlow)
            {
                case AuthorizationFlow.AuthorizationCodeFlow:
                    endpointResult = await _getAuthorizationCodeOperation
                        .Execute(parameter, claimsPrincipal, client, issuerName, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case AuthorizationFlow.ImplicitFlow:
                    endpointResult = await _getTokenViaImplicitWorkflowOperation.Execute(
                            parameter,
                            claimsPrincipal,
                            client,
                            issuerName,
                            CancellationToken.None)
                        .ConfigureAwait(false);
                    break;
                case AuthorizationFlow.HybridFlow:
                    endpointResult = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation
                        .Execute(parameter, claimsPrincipal, client, issuerName, cancellationToken)
                        .ConfigureAwait(false);
                    break;
            }

            await _eventPublisher.Publish(
                    new AuthorizationGranted(Id.Create(), claimsPrincipal?.Identity.Name, client?.ClientId, DateTime.UtcNow))
                .ConfigureAwait(false);
            endpointResult.ProcessId = processId;
            endpointResult.Amr = _resourceOwnerServices.GetAmrs().ToArray().GetAmr(parameter.AmrValues.ToArray());
            return endpointResult;
        }
    }
}
