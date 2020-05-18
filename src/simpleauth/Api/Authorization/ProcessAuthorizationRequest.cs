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
    using Extensions;
    using Parameters;
    using Results;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal sealed class ProcessAuthorizationRequest
    {
        private readonly IClientStore _clientStore;
        private readonly IConsentRepository _consentRepository;
        private readonly IJwksStore _jwksStore;

        public ProcessAuthorizationRequest(
            IClientStore clientStore, IConsentRepository consentRepository, IJwksStore jwksStore)
        {
            _clientStore = clientStore;
            _consentRepository = consentRepository;
            _jwksStore = jwksStore;
        }

        public async Task<EndpointResult> Process(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            Client client,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
            Consent confirmedConsent = null;
            if (endUserIsAuthenticated)
            {
                confirmedConsent =
                    await GetResourceOwnerConsent(claimsPrincipal, authorizationParameter, cancellationToken)
                        .ConfigureAwait(false);
            }

            EndpointResult result = null;
            var prompts = authorizationParameter.Prompt.ParsePrompts();
            if (prompts == null || !prompts.Any())
            {
                prompts = new List<string>();
                if (!endUserIsAuthenticated)
                {
                    prompts.Add(PromptParameters.Login);
                }
                else
                {
                    prompts.Add(confirmedConsent == null ? PromptParameters.Consent : PromptParameters.None);
                }
            }

            var redirectionUrls = client.GetRedirectionUrls(authorizationParameter.RedirectUrl);
            if (!redirectionUrls.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.RedirectUrlIsNotValid, authorizationParameter.RedirectUrl),
                    authorizationParameter.State);
            }

            var scopeValidationResult = authorizationParameter.Scope.Check(client);
            if (!scopeValidationResult.IsValid)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidScope,
                    scopeValidationResult.ErrorMessage,
                    authorizationParameter.State);
            }

            // TODO: Investigate
            //if (!scopeValidationResult.Scopes.Contains(CoreConstants.StandardScopes.OpenId.Name))
            //{
            //    throw new SimpleAuthExceptionWithState(
            //        ErrorCodes.InvalidScope,
            //        string.Format(Strings.TheScopesNeedToBeSpecified, CoreConstants.StandardScopes.OpenId.Name),
            //        authorizationParameter.State);
            //}

            var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
            if (!responseTypes.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.MissingParameter,
                        CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    authorizationParameter.State);
            }

            if (!client.CheckResponseTypes(responseTypes.ToArray()))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    string.Format(
                        Strings.TheClientDoesntSupportTheResponseType,
                        authorizationParameter.ClientId,
                        string.Join(",", responseTypes)),
                    authorizationParameter.State);
            }

            // Check if the user connection is still valid.
            if (endUserIsAuthenticated && !authorizationParameter.MaxAge.Equals(default))
            {
                var authenticationDateTimeClaim =
                    claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
                if (authenticationDateTimeClaim != null)
                {
                    var maxAge = authorizationParameter.MaxAge;
                    var currentDateTimeUtc = DateTimeOffset.UtcNow.ConvertToUnixTimestamp();
                    var authenticationDateTime = long.Parse(authenticationDateTimeClaim.Value);
                    if (maxAge < currentDateTimeUtc - authenticationDateTime)
                    {
                        result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
                        result.RedirectInstruction.Action = SimpleAuthEndPoints.AuthenticateIndex;
                    }
                }
            }

            if (result == null)
            {
                result = ProcessPromptParameters(prompts, claimsPrincipal, authorizationParameter, confirmedConsent);

                await ProcessIdTokenHint(
                        result,
                        authorizationParameter,
                        prompts,
                        claimsPrincipal,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }

        private async Task ProcessIdTokenHint(
            EndpointResult endpointResult,
            AuthorizationParameter authorizationParameter,
            ICollection<string> prompts,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint)
                && prompts.Contains(PromptParameters.None)
                && endpointResult.Type == ActionResultType.RedirectToCallBackUrl)
            {
                var handler = new JwtSecurityTokenHandler();
                var token = authorizationParameter.IdTokenHint;
                var canRead = handler.CanReadToken(token);
                if (!canRead)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequest,
                        Strings.TheIdTokenHintParameterIsNotAValidToken,
                        authorizationParameter.State);
                }

                var client = await _clientStore.GetById(authorizationParameter.ClientId, cancellationToken)
                    .ConfigureAwait(false);
                var validationParameters = await client
                    .CreateValidationParameters(_jwksStore, issuerName, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                handler.ValidateToken(token, validationParameters, out var securityToken);
                var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;

                if (jwsPayload?.Aud == null || !jwsPayload.Aud.Contains(issuerName))
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequest,
                        Strings.TheIdentityTokenDoesntContainSimpleAuthAsAudience,
                        authorizationParameter.State);
                }

                var currentSubject = string.Empty;
                var expectedSubject = jwsPayload.GetClaimValue(OpenIdClaimTypes.Subject);
                if (claimsPrincipal != null && claimsPrincipal.IsAuthenticated())
                {
                    currentSubject = claimsPrincipal.GetSubject();
                }

                if (currentSubject != expectedSubject)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequest,
                        Strings.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken,
                        authorizationParameter.State);
                }
            }
        }

        private EndpointResult ProcessPromptParameters(
            ICollection<string> prompts,
            ClaimsPrincipal principal,
            AuthorizationParameter authorizationParameter,
            Consent confirmedConsent)
        {
            if (prompts == null || !prompts.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt,
                    authorizationParameter.State);
            }

            var endUserIsAuthenticated = IsAuthenticated(principal);

            // Raise "login_required" exception : if the prompt authorizationParameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameters.None))
            {
                if (!endUserIsAuthenticated)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.LoginRequiredCode,
                        Strings.TheUserNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                if (confirmedConsent == null)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InteractionRequiredCode,
                        Strings.TheUserNeedsToGiveHisConsent,
                        authorizationParameter.State);
                }

                var result = EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                return result;
            }

            // Redirects to the authentication screen
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
            if (prompts.Contains(PromptParameters.Login))
            {
                var result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = SimpleAuthEndPoints.AuthenticateIndex;
                return result;
            }

            if (prompts.Contains(PromptParameters.Consent))
            {
                var result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
                if (!endUserIsAuthenticated)
                {
                    result.RedirectInstruction.Action = SimpleAuthEndPoints.AuthenticateIndex;
                    return result;
                }

                result.RedirectInstruction.Action = SimpleAuthEndPoints.ConsentIndex;
                return result;
            }

            throw new SimpleAuthExceptionWithState(
                ErrorCodes.InvalidRequest,
                string.Format(Strings.ThePromptParameterIsNotSupported, string.Join(",", prompts)),
                authorizationParameter.State);
        }

        private async Task<Consent> GetResourceOwnerConsent(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            CancellationToken cancellationToken)
        {
            var subject = claimsPrincipal.GetSubject();
            return await _consentRepository.GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);
        }

        private static bool IsAuthenticated(ClaimsPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated ?? false;
        }
    }
}
