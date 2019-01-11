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
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Parameters;
    using Results;
    using Shared.Models;
    using Shared.Repositories;
    using Validators;
    using JwtConstants = Shared.JwtConstants;

    internal sealed class ProcessAuthorizationRequest
    {
        private readonly ParameterParserHelper _parameterParserHelper;
        private readonly ClientValidator _clientValidator;
        private readonly ScopeValidator _scopeValidator;
        private readonly IConsentHelper _consentHelper;
        private readonly IClientStore _clientStore;

        public ProcessAuthorizationRequest(IClientStore clientStore, IConsentHelper consentHelper)
        {
            _clientStore = clientStore;
            _parameterParserHelper = new ParameterParserHelper();
            _clientValidator = new ClientValidator();
            _scopeValidator = new ScopeValidator();
            _consentHelper = consentHelper;
        }

        public async Task<EndpointResult> ProcessAsync(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName)
        {
            var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
            Consent confirmedConsent = null;
            if (endUserIsAuthenticated)
            {
                confirmedConsent = await GetResourceOwnerConsent(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            }

            //var serializedAuthorizationParameter = authorizationParameter.SerializeWithJavascript();
            //_oauthEventSource.StartProcessingAuthorizationRequest(serializedAuthorizationParameter);
            EndpointResult result = null;
            var prompts = _parameterParserHelper.ParsePrompts(authorizationParameter.Prompt);
            if (prompts == null || !prompts.Any())
            {
                prompts = new List<PromptParameter>();
                if (!endUserIsAuthenticated)
                {
                    prompts.Add(PromptParameter.login);
                }
                else
                {
                    prompts.Add(confirmedConsent == null ? PromptParameter.consent : PromptParameter.none);
                }
            }

            var redirectionUrls = _clientValidator.GetRedirectionUrls(client, authorizationParameter.RedirectUrl);
            if (!redirectionUrls.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationParameter.RedirectUrl),
                    authorizationParameter.State);
            }

            var scopeValidationResult = _scopeValidator.Check(authorizationParameter.Scope, client);
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
            //        string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, CoreConstants.StandardScopes.OpenId.Name),
            //        authorizationParameter.State);
            //}

            var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            if (!responseTypes.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName),
                    authorizationParameter.State);
            }

            if (!_clientValidator.CheckResponseTypes(client, responseTypes.ToArray()))
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        authorizationParameter.ClientId,
                        string.Join(",", responseTypes)),
                    authorizationParameter.State);
            }

            // Check if the user connection is still valid.
            if (endUserIsAuthenticated && !authorizationParameter.MaxAge.Equals(default))
            {
                var authenticationDateTimeClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
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
                result = ProcessPromptParameters(
                    prompts,
                    claimsPrincipal,
                    authorizationParameter,
                    confirmedConsent);

                await ProcessIdTokenHint(result,
                    authorizationParameter,
                    prompts,
                    claimsPrincipal,
                    issuerName).ConfigureAwait(false);
            }

            //var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            //var actionName = result.RedirectInstruction == null
            //    ? string.Empty
            //    : Enum.GetName(typeof(SimpleAuthEndPoints), result.RedirectInstruction.Action);
            //_oauthEventSource.EndProcessingAuthorizationRequest(
            //    serializedAuthorizationParameter,
            //    actionTypeName,
            //    actionName);

            return result;
        }

        private async Task ProcessIdTokenHint(
            EndpointResult endpointResult,
            AuthorizationParameter authorizationParameter,
            ICollection<PromptParameter> prompts,
            ClaimsPrincipal claimsPrincipal,
            string issuerName)
        {
            if (!string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint) &&
                prompts.Contains(PromptParameter.none) &&
                endpointResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var handler = new JwtSecurityTokenHandler();
                var token = authorizationParameter.IdTokenHint;
                var canRead = handler.CanReadToken(token);
                if (!canRead)
                {
                    throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheIdTokenHintParameterIsNotAValidToken,
                            authorizationParameter.State);
                }

                var client = await _clientStore.GetById(authorizationParameter.ClientId).ConfigureAwait(false);
                handler.ValidateToken(token, client.CreateValidationParameters(issuerName), out var securityToken);
                var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;
                //string jwsToken;
                //if (token.IsJweToken())
                //{

                //    // jwsToken = await _jwtParser.DecryptAsync(token).ConfigureAwait(false);
                //    if (string.IsNullOrWhiteSpace(jwsToken))
                //    {
                //        throw new SimpleAuthExceptionWithState(
                //            ErrorCodes.InvalidRequestCode,
                //            ErrorDescriptions.TheIdTokenHintParameterCannotBeDecrypted,
                //            authorizationParameter.State);
                //    }
                //}
                //else
                //{
                //    jwsToken = token;
                //}

                //var jwsPayload = await _jwtParser.UnSignAsync(jwsToken).ConfigureAwait(false);
                //if (jwsPayload == null)
                //{
                //    throw new SimpleAuthExceptionWithState(
                //        ErrorCodes.InvalidRequestCode,
                //        ErrorDescriptions.TheSignatureOfIdTokenHintParameterCannotBeChecked,
                //        authorizationParameter.State);
                //}

                if (jwsPayload?.Aud == null || !jwsPayload.Aud.Contains(issuerName))
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheIdentityTokenDoesntContainSimpleAuthAsAudience,
                        authorizationParameter.State);
                }

                var currentSubject = string.Empty;
                var expectedSubject = jwsPayload.GetClaimValue(JwtConstants.StandardResourceOwnerClaimNames.Subject);
                if (claimsPrincipal != null && claimsPrincipal.IsAuthenticated())
                {
                    currentSubject = claimsPrincipal.GetSubject();
                }

                if (currentSubject != expectedSubject)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken,
                        authorizationParameter.State);
                }
            }
        }

        private EndpointResult ProcessPromptParameters(ICollection<PromptParameter> prompts, ClaimsPrincipal principal, AuthorizationParameter authorizationParameter, Consent confirmedConsent)
        {
            if (prompts == null || !prompts.Any())
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt,
                    authorizationParameter.State);
            }

            var endUserIsAuthenticated = IsAuthenticated(principal);

            // Raise "login_required" exception : if the prompt authorizationParameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameter.none))
            {
                if (!endUserIsAuthenticated)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.LoginRequiredCode,
                        ErrorDescriptions.TheUserNeedsToBeAuthenticated,
                        authorizationParameter.State);
                }

                if (confirmedConsent == null)
                {
                    throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InteractionRequiredCode,
                            ErrorDescriptions.TheUserNeedsToGiveHisConsent,
                            authorizationParameter.State);
                }

                var result = EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                return result;
            }

            // Redirects to the authentication screen 
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
            if (prompts.Contains(PromptParameter.login))
            {
                var result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = SimpleAuthEndPoints.AuthenticateIndex;
                return result;
            }

            if (prompts.Contains(PromptParameter.consent))
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
                ErrorCodes.InvalidRequestCode,
                string.Format(ErrorDescriptions.ThePromptParameterIsNotSupported, string.Join(",", prompts)),
                authorizationParameter.State);
        }

        private async Task<Consent> GetResourceOwnerConsent(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter)
        {
            var subject = claimsPrincipal.GetSubject();
            return await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter).ConfigureAwait(false);
        }

        private static bool IsAuthenticated(ClaimsPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated ?? false;
        }
    }
}
