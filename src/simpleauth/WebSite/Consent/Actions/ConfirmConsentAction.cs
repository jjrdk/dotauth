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
    using Api.Authorization;
    using Common;
    using Exceptions;
    using Extensions;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.Logging;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Openid;

    internal class ConfirmConsentAction
    {
        private readonly IConsentRepository _consentRepository;
        private readonly IClientStore _clientRepository;
        private readonly IScopeRepository _scopeRepository;
        private readonly IResourceOwnerStore _resourceOwnerRepository;
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IEventPublisher _eventPublisher;

        public ConfirmConsentAction(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IConsentRepository consentRepository,
            IClientStore clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerStore resourceOwnerRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientRepository,
                consentRepository,
                jwksStore,
                eventPublisher);
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// This method is executed when the user confirm the consent
        /// 1). If there's already consent confirmed in the past by the resource owner
        /// 1).* then we generate an authorization code and redirects to the callback.
        /// 2). If there's no consent then we insert it and the authorization code is returned
        ///  2°.* to the callback url.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant-type</param>
        /// <param name="claimsPrincipal">Resource owner's claims</param>
        /// <param name="issuerName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Redirects the authorization code to the callback.</returns>
        public async Task<EndpointResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken)
                .ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException($"the client id {authorizationParameter.ClientId} doesn't exist");
            }

            var subject = claimsPrincipal.GetSubject();
            var assignedConsent = await _consentRepository
                .GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);
            // Insert a new consent.
            if (assignedConsent == null)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyIdentityTokenClaimParameter() || claimsParameter.IsAnyUserInfoClaimParameter())
                {
                    // A consent can be given to a set of claims
                    assignedConsent = new Consent
                    {
                        Id = Id.Create(),
                        Client = client,
                        ResourceOwner =
                            await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false),
                        Claims = claimsParameter.GetClaimNames()
                    };
                }
                else
                {
                    // A consent can be given to a set of scopes
                    assignedConsent = new Consent
                    {
                        Id = Id.Create(),
                        Client = client,
                        GrantedScopes =
                            (await GetScopes(authorizationParameter.Scope, cancellationToken).ConfigureAwait(false))
                            .ToArray(),
                        ResourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken)
                            .ConfigureAwait(false),
                    };
                }

                // A consent can be given to a set of claims
                await _consentRepository.Insert(assignedConsent, cancellationToken).ConfigureAwait(false);

                await _eventPublisher.Publish(
                        new ConsentAccepted(
                            Id.Create(),
                            subject,
                            authorizationParameter.ClientId,
                            assignedConsent.GrantedScopes,
                            DateTime.UtcNow))
                    .ConfigureAwait(false);
            }

            var result = EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
            await _generateAuthorizationResponse.Generate(
                    result,
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);

            // If redirect to the callback and the responde mode has not been set.
            if (result.Type == ActionResultType.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseModes.None)
                {
                    var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
            }

            return result;
        }

        private async Task<string[]> GetScopes(string concatenateListOfScopes, CancellationToken cancellationToken)
        {
            var scopeNames = concatenateListOfScopes.ParseScopes();
            var scopes = await _scopeRepository.SearchByNames(cancellationToken, scopeNames).ConfigureAwait(false);
            return scopes.Select(x => x.Name).ToArray();
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<string> responseTypes, string state)
        {
            if (responseTypes == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys.SingleOrDefault(
                k => k.Length == responseTypes.Count && k.All(responseTypes.Contains));
            if (record == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
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
