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

using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class DisplayConsentAction : IDisplayConsentAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IClientStore _clientRepository;
        private readonly IConsentHelper _consentHelper;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IActionResultFactory _actionResultFactory;

        public DisplayConsentAction(
            IScopeRepository scopeRepository,
            IClientStore clientRepository,
            IConsentHelper consentHelper,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentHelper = consentHelper;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }

        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the parameter
        /// Those informations are used to create the consent screen.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant type parameter.</param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="client">Information about the client</param>
        /// <param name="allowedScopes">Allowed scopes</param>
        /// <param name="allowedClaims">Allowed claims</param>
        /// <returns>Action result.</returns>
        public async Task<DisplayContentResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal?.Identity == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var client = await _clientRepository.GetById(authorizationParameter.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ClientIsNotValid, authorizationParameter.ClientId),
                    authorizationParameter.State);
            }

            EndpointResult endpointResult;
            var subject = claimsPrincipal.GetSubject();
            var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter).ConfigureAwait(false);
            // If there's already a consent then redirect to the callback
            if (assignedConsent != null)
            {
                endpointResult = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                await _generateAuthorizationResponse.ExecuteAsync(endpointResult, authorizationParameter, claimsPrincipal, client, issuerName).ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                endpointResult.RedirectInstruction.ResponseMode = responseMode;
                return new DisplayContentResult
                {
                    EndpointResult = endpointResult
                };
            }

            ICollection<string> allowedClaims = null;
            ICollection<Scope> allowedScopes = null;
            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                claimsParameter.IsAnyUserInfoClaimParameter())
            {
                allowedClaims = claimsParameter.GetClaimNames();
            }
            else
            {
                allowedScopes = (await GetScopes(authorizationParameter.Scope).ConfigureAwait(false))
                    .Where(s => s.IsDisplayedInConsent)
                    .ToList();
            }

            endpointResult = _actionResultFactory.CreateAnEmptyActionResultWithOutput();
            return new DisplayContentResult
            {
                AllowedClaims = allowedClaims,
                Scopes = allowedScopes,
                EndpointResult = endpointResult,
                Client = client
            };
        }

        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private async Task<IEnumerable<Scope>> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = concatenateListOfScopes.Split(' ');
            return await _scopeRepository.SearchByNames(scopeNames).ConfigureAwait(false);
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state)
        {
            if (responseTypes == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Count == responseTypes.Count && k.All(key => responseTypes.Contains(key)));
            if (record == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return CoreConstants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
