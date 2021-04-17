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
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal sealed class ProcessAuthorizationRequest
    {
        private readonly IClientStore _clientStore;
        private readonly IConsentRepository _consentRepository;
        private readonly IJwksStore _jwksStore;
        private readonly ILogger _logger;

        public ProcessAuthorizationRequest(
            IClientStore clientStore,
            IConsentRepository consentRepository,
            IJwksStore jwksStore,
            ILogger logger)
        {
            _clientStore = clientStore;
            _consentRepository = consentRepository;
            _jwksStore = jwksStore;
            _logger = logger;
        }

        public async Task<EndpointResult> Process(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            Client client,
            string issuerName,
            CancellationToken cancellationToken = default)
        {
            var endUserIsAuthenticated = IsAuthenticated(claimsPrincipal);
            Consent? confirmedConsent = null;
            if (endUserIsAuthenticated)
            {
                confirmedConsent =
                    await GetResourceOwnerConsent(claimsPrincipal, authorizationParameter, cancellationToken)
                        .ConfigureAwait(false);
            }

            EndpointResult? result = null;
            var prompts = authorizationParameter.Prompt.ParsePrompts();
            if (prompts.Count == 0)
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

            if (authorizationParameter.RedirectUrl == null
                || client.GetRedirectionUrls(authorizationParameter.RedirectUrl).Length == 0)
            {
                var message = string.Format(Strings.RedirectUrlIsNotValid, authorizationParameter.RedirectUrl);
                _logger.LogError(message);
                return EndpointResult.CreateBadRequestResult(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = message,
                    Status = HttpStatusCode.BadRequest
                });
            }

            var scopeValidationResult = authorizationParameter.Scope.Check(client);
            if (!scopeValidationResult.IsValid)
            {
                _logger.LogError(scopeValidationResult.ErrorMessage!);
                return EndpointResult.CreateBadRequestResult(new ErrorDetails
                {
                    Title = ErrorCodes.InvalidScope,
                    Detail = scopeValidationResult.ErrorMessage!,
                    Status = HttpStatusCode.BadRequest
                });
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
                var message = string.Format(
                    Strings.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName);
                _logger.LogError(
                    message);
                return EndpointResult.CreateBadRequestResult(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            if (!client.CheckResponseTypes(responseTypes.ToArray()))
            {
                var message = string.Format(
                    Strings.TheClientDoesntSupportTheResponseType,
                    authorizationParameter.ClientId,
                    string.Join(",", responseTypes));
                _logger.LogError(message);
                return EndpointResult.CreateBadRequestResult(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest, Detail = message, Status = HttpStatusCode.BadRequest
                    });
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
                        result = EndpointResult.CreateAnEmptyActionResultWithRedirection(SimpleAuthEndPoints.AuthenticateIndex);
                    }
                }
            }

            if (result != null)
            {
                return result;
            }

            result = ProcessPromptParameters(prompts, claimsPrincipal, authorizationParameter, confirmedConsent);
            if (result.Type == ActionResultType.BadRequest)
            {
                return result;
            }
            var success = await ProcessIdTokenHint(
                    result,
                    authorizationParameter,
                    prompts,
                    claimsPrincipal,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            return success switch
            {
                Option.Error e => EndpointResult.CreateBadRequestResult(e.Details),
                _ => result
            };

        }

        private async Task<Option> ProcessIdTokenHint(
            EndpointResult endpointResult,
            AuthorizationParameter authorizationParameter,
            ICollection<string> prompts,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationParameter.IdTokenHint)
                || !prompts.Contains(PromptParameters.None)
                || endpointResult.Type != ActionResultType.RedirectToCallBackUrl)
            {
                return new Option.Success();
            }

            var handler = new JwtSecurityTokenHandler();
            var token = authorizationParameter.IdTokenHint;
            var canRead = handler.CanReadToken(token);
            if (!canRead)
            {
                _logger.LogError(Strings.TheIdTokenHintParameterIsNotAValidToken);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.TheIdTokenHintParameterIsNotAValidToken,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var client = authorizationParameter.ClientId == null
                ? null
                : await _clientStore.GetById(authorizationParameter.ClientId, cancellationToken)
                    .ConfigureAwait(false);
            var validationParameters = client == null
                ? null
                : await client.CreateValidationParameters(
                        _jwksStore,
                        issuerName,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            handler.ValidateToken(token, validationParameters, out var securityToken);
            var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;

            if (jwsPayload?.Aud == null || !jwsPayload.Aud.Contains(issuerName))
            {
                _logger.LogError(Strings.TheIdentityTokenDoesntContainSimpleAuthAsAudience);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.TheIdentityTokenDoesntContainSimpleAuthAsAudience,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var currentSubject = string.Empty;
            var expectedSubject = jwsPayload.GetClaimValue(OpenIdClaimTypes.Subject);
            if (claimsPrincipal.IsAuthenticated())
            {
                currentSubject = claimsPrincipal.GetSubject();
            }

            if (currentSubject != expectedSubject)
            {
                _logger.LogError(Strings.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.TheCurrentAuthenticatedUserDoesntMatchWithTheIdentityToken,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            return new Option.Success();
        }

        private EndpointResult ProcessPromptParameters(
            ICollection<string> prompts,
            ClaimsPrincipal principal,
            AuthorizationParameter authorizationParameter,
            Consent? confirmedConsent)
        {
            if (prompts.Count == 0)
            {
                _logger.LogError(Strings.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt);
                return EndpointResult.CreateBadRequestResult(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = Strings.TheAuthorizationRequestCannotBeProcessedBecauseThereIsNotValidPrompt,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var endUserIsAuthenticated = IsAuthenticated(principal);

            // Raise "login_required" exception : if the prompt authorizationParameter is "none" AND the user is not authenticated
            // Raise "interaction_required" exception : if there's no consent from the user.
            if (prompts.Contains(PromptParameters.None))
            {
                if (!endUserIsAuthenticated)
                {
                    _logger.LogError(Strings.TheUserNeedsToBeAuthenticated);
                    return EndpointResult.CreateBadRequestResult(
                        new ErrorDetails
                        {
                            Title = ErrorCodes.LoginRequiredCode,
                            Detail = Strings.TheUserNeedsToBeAuthenticated,
                            Status = HttpStatusCode.BadRequest
                        });
                }

                if (confirmedConsent != null)
                {
                    return EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                }

                _logger.LogError(Strings.TheUserNeedsToGiveHisConsent);
                return EndpointResult.CreateBadRequestResult(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InteractionRequiredCode,
                        Detail = Strings.TheUserNeedsToGiveHisConsent,
                        Status = HttpStatusCode.BadRequest
                    });

            }

            // Redirects to the authentication screen
            // if the "prompt" authorizationParameter is equal to "login" OR
            // The user is not authenticated AND the prompt authorizationParameter is different from "none"
            if (prompts.Contains(PromptParameters.Login))
            {
                var result = EndpointResult.CreateAnEmptyActionResultWithRedirection(SimpleAuthEndPoints.AuthenticateIndex);
                return result;
            }

            if (prompts.Contains(PromptParameters.Consent))
            {
                return EndpointResult.CreateAnEmptyActionResultWithRedirection(
                    endUserIsAuthenticated ? SimpleAuthEndPoints.ConsentIndex : SimpleAuthEndPoints.AuthenticateIndex);
            }

            throw new SimpleAuthExceptionWithState(
                ErrorCodes.InvalidRequest,
                string.Format(Strings.ThePromptParameterIsNotSupported, string.Join(",", prompts)),
                authorizationParameter.State);
        }

        private async Task<Consent?> GetResourceOwnerConsent(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            CancellationToken cancellationToken)
        {
            var subject = claimsPrincipal.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                return null;
            }

            return await _consentRepository.GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);
        }

        private static bool IsAuthenticated(ClaimsPrincipal principal)
        {
            return principal.Identity?.IsAuthenticated ?? false;
        }
    }
}
