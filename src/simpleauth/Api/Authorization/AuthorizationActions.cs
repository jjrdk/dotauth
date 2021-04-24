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
    using Parameters;
    using Results;
    using Shared.Events.OAuth;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Events;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;

    internal class AuthorizationActions
    {
        private readonly GetAuthorizationCodeOperation _getAuthorizationCodeOperation;
        private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        private readonly GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

        private readonly AuthorizationCodeGrantTypeParameterAuthEdpValidator
            _authorizationCodeGrantTypeParameterValidator;

        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger _logger;
        private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;

        public AuthorizationActions(
            IAuthorizationCodeStore authorizationCodeStore,
            IClientStore clientStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IConsentRepository consentRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher,
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
            ILogger logger)
        {
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientStore,
                consentRepository,
                jwksStore,
                eventPublisher,
                logger);
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                clientStore,
                consentRepository,
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                jwksStore,
                eventPublisher,
                logger);
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                    clientStore,
                    consentRepository,
                    authorizationCodeStore,
                    tokenStore,
                    scopeRepository,
                    jwksStore,
                    eventPublisher,
                    logger);
            _authorizationCodeGrantTypeParameterValidator =
                new AuthorizationCodeGrantTypeParameterAuthEdpValidator(clientStore, logger);
            _eventPublisher = eventPublisher;
            _logger = logger;
            _resourceOwnerServices = resourceOwnerServices.ToArray();
        }

        public async Task<EndpointResult> GetAuthorization(
            AuthorizationParameter parameter,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var processId = Id.Create();

            var result = await _authorizationCodeGrantTypeParameterValidator.Validate(parameter, cancellationToken)
                .ConfigureAwait(false);
            Client client = null!;
            switch (result)
            {
                case Option<Client>.Error error:
                    return EndpointResult.CreateBadRequestResult(error.Details);
                case Option<Client>.Result r:
                    client = r.Item;
                    break;
            }

            EndpointResult? endpointResult = null;

            if (client.RequirePkce
                && (string.IsNullOrWhiteSpace(parameter.CodeChallenge) || parameter.CodeChallengeMethod == null))
            {
                _logger.LogError(string.Format(Strings.TheClientRequiresPkce, parameter.ClientId));
                return EndpointResult.CreateBadRequestResult(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(Strings.TheClientRequiresPkce, parameter.ClientId),
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var responseTypes = parameter.ResponseType.ParseResponseTypes();
            var authorizationFlow = responseTypes.GetAuthorizationFlow(parameter.State);
            endpointResult = authorizationFlow switch
            {
                Option<AuthorizationFlow>.Error e => EndpointResult.CreateBadRequestResult(e.Details),
                Option<AuthorizationFlow>.Result
                { Item: AuthorizationFlow.AuthorizationCodeFlow } => await _getAuthorizationCodeOperation
                    .Execute(parameter, claimsPrincipal, client, issuerName, cancellationToken)
                    .ConfigureAwait(false),
                Option<AuthorizationFlow>.Result
                { Item: AuthorizationFlow.ImplicitFlow } => await _getTokenViaImplicitWorkflowOperation.Execute(
                        parameter,
                        claimsPrincipal,
                        client,
                        issuerName,
                        CancellationToken.None)
                    .ConfigureAwait(false),
                Option<AuthorizationFlow>.Result
                { Item: AuthorizationFlow.HybridFlow } => await
                    _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                            parameter,
                            claimsPrincipal,
                            client,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false),
                _ => endpointResult
            };

            await _eventPublisher.Publish(
                    new AuthorizationGranted(Id.Create(), claimsPrincipal.Identity?.Name, client.ClientId, DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            var option = _resourceOwnerServices.GetAmrs().ToArray().GetAmr(parameter.AmrValues.ToArray());
            if (option is Option<string>.Error er)
            {
                return EndpointResult.CreateBadRequestResult(er.Details);
            }
            return endpointResult! with
            {
                ProcessId = processId,
                Amr = ((Option<string>.Result)option).Item
            };
        }
    }
}
