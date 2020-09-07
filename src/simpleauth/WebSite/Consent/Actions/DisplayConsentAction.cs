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

namespace SimpleAuth.WebSite.Consent.Actions
{
    using System;
    using Api.Authorization;
    using Common;
    using Exceptions;
    using Extensions;
    using Parameters;
    using Results;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Events;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;

    internal class DisplayConsentAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IClientStore _clientRepository;
        private readonly IConsentRepository _consentRepository;
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;

        public DisplayConsentAction(
            IScopeRepository scopeRepository,
            IClientStore clientRepository,
            IConsentRepository consentRepository,
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentRepository = consentRepository;
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientRepository,
                consentRepository,
                jwksStore,
                eventPublisher);
        }

        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the parameter
        /// Those information are used to create the consent screen.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant type parameter.</param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="issuerName"></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>Action resultKind.</returns>
        public async Task<DisplayContentResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var client = authorizationParameter.ClientId == null
                ? null
                : await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken)
                    .ConfigureAwait(false);
            if (client == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.ClientIsNotValid, authorizationParameter.ClientId),
                    authorizationParameter.State);
            }

            EndpointResult endpointResult;
            var subject = claimsPrincipal.GetSubject()!;
            var assignedConsent = await _consentRepository
                .GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);
            // If there's already a consent then redirect to the callback
            if (assignedConsent != null)
            {
                endpointResult = EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                await _generateAuthorizationResponse.Generate(
                        endpointResult,
                        authorizationParameter,
                        claimsPrincipal,
                        client,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseModes.None)
                {
                    var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                endpointResult.RedirectInstruction!.ResponseMode = responseMode;
                return new DisplayContentResult(endpointResult);
            }

            ICollection<string> allowedClaims = Array.Empty<string>();
            ICollection<Scope> allowedScopes = Array.Empty<Scope>();
            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter.IsAnyIdentityTokenClaimParameter() || claimsParameter.IsAnyUserInfoClaimParameter())
            {
                allowedClaims = claimsParameter.GetClaimNames();
            }
            else
            {
                allowedScopes =
                    (await GetScopes(authorizationParameter.Scope!, cancellationToken).ConfigureAwait(false))
                    .Where(s => s.IsDisplayedInConsent)
                    .ToList();
            }

            endpointResult = EndpointResult.CreateAnEmptyActionResultWithOutput();
            return new DisplayContentResult(client, allowedScopes, allowedClaims, endpointResult);
        }

        private async Task<IEnumerable<Scope>> GetScopes(
            string concatenateListOfScopes,
            CancellationToken cancellationToken)
        {
            var scopeNames = concatenateListOfScopes.Split(' ');
            return await _scopeRepository.SearchByNames(cancellationToken, scopeNames).ConfigureAwait(false);
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<string> responseTypes, string? state)
        {
            if (responseTypes == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys.SingleOrDefault(
                k => k.Length == responseTypes.Count && k.All(responseTypes.Contains));
            if (record == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return CoreConstants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static string GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
