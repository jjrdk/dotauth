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

namespace SimpleAuth.Controllers
{
    using Api.Authorization;
    using Exceptions;
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Repositories;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Filters;

    /// <summary>
    /// Defines the authorization controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(CoreConstants.EndPoints.Authorization)]
    [ThrottleFilter]
    public class AuthorizationController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IClientStore _clientStore;
        private readonly IJwksStore _jwksStore;
        private readonly AuthorizationActions _authorizationActions;
        private readonly IDataProtector _dataProtector;
        private readonly IAuthenticationService _authenticationService;
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationController"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="resourceOwnerServices">The resource owner services.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="tokenStore">The token store.</param>
        /// <param name="scopeRepository">The scope repository.</param>
        /// <param name="authorizationCodeStore">The authorization code store.</param>
        /// <param name="consentRepository">The consent repository.</param>
        /// <param name="jwksStore"></param>
        /// <param name="dataProtectionProvider">The data protection provider.</param>
        /// <param name="authenticationService">The authentication service.</param>
        public AuthorizationController(
            HttpClient httpClient,
            IEventPublisher eventPublisher,
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
            IClientStore clientStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IAuthorizationCodeStore authorizationCodeStore,
            IConsentRepository consentRepository,
            IJwksStore jwksStore,
            IDataProtectionProvider dataProtectionProvider,
            IAuthenticationService authenticationService)
        {
            _httpClient = httpClient;
            _clientStore = clientStore;
            _jwksStore = jwksStore;
            _authorizationActions = new AuthorizationActions(
                authorizationCodeStore,
                clientStore,
                tokenStore,
                scopeRepository,
                consentRepository,
                jwksStore,
                eventPublisher,
                resourceOwnerServices);
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Handles the authorization request.
        /// </summary>
        /// <param name="authorizationRequest">The authorization request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] AuthorizationRequest authorizationRequest, CancellationToken cancellationToken)
        {
            var originUrl = this.GetOriginUrl();
            var sessionId = GetSessionId();

            authorizationRequest = await ResolveAuthorizationRequest(authorizationRequest, cancellationToken).ConfigureAwait(false);
            authorizationRequest.origin_url = originUrl;
            authorizationRequest.session_id = sessionId;
            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, CookieNames.CookieName)
                .ConfigureAwait(false);
            var parameter = authorizationRequest.ToParameter();
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _authorizationActions.GetAuthorization(parameter, authenticatedUser, issuerName, cancellationToken)
                .ConfigureAwait(false);

            switch (actionResult.Type)
            {
                case ActionResultType.RedirectToCallBackUrl:
                    {
                        return authorizationRequest.redirect_uri.CreateRedirectHttpTokenResponse(
                            actionResult.GetRedirectionParameters(),
                            actionResult.RedirectInstruction.ResponseMode);
                    }
                case ActionResultType.RedirectToAction:
                    {
                        if (actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.AuthenticateIndex
                            || actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.ConsentIndex)
                        {
                            // Force the resource owner to be re-authenticated
                            if (actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.AuthenticateIndex)
                            {
                                authorizationRequest.prompt = PromptParameters.Login;
                            }

                            // Set the process id into the request.
                            if (!string.IsNullOrWhiteSpace(actionResult.ProcessId))
                            {
                                authorizationRequest.aggregate_id = actionResult.ProcessId;
                            }

                            // Add the encoded request into the query string
                            var encryptedRequest = _dataProtector.Protect(authorizationRequest);
                            actionResult.RedirectInstruction.AddParameter(
                                StandardAuthorizationResponseNames.AuthorizationCodeName,
                                encryptedRequest);
                        }

                        var url = GetRedirectionUrl(Request, actionResult.Amr, actionResult.RedirectInstruction.Action);
                        var uri = new Uri(url);
                        var redirectionUrl = uri.AddParametersInQuery(actionResult.GetRedirectionParameters());
                        return new RedirectResult(redirectionUrl.AbsoluteUri);
                    }
                //case ActionResultType.Output:
                //case ActionResultType.None:
                default:
                    return null;
            }
        }

        private string GetSessionId()
        {
            return !Request.Cookies.ContainsKey(CoreConstants.SessionId)
                ? Id.Create()
                : Request.Cookies[CoreConstants.SessionId];
        }

        private async Task<AuthorizationRequest> GetAuthorizationRequestFromJwt(
            string token,
            string clientId,
            CancellationToken cancellationToken)
        {
            var client = await _clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
            var validationParameters = await client.CreateValidationParameters(_jwksStore).ConfigureAwait(false);
            _handler.ValidateToken(token, validationParameters, out var securityToken);

            return (securityToken as JwtSecurityToken)?.Payload?.ToAuthorizationRequest();
        }

        private static string GetRedirectionUrl(
            Microsoft.AspNetCore.Http.HttpRequest request,
            string amr,
            SimpleAuthEndPoints simpleAuthEndPoints)
        {
            var uri = request.GetAbsoluteUriWithVirtualPath();
            var partialUri = simpleAuthEndPoints switch
            {
                SimpleAuthEndPoints.AuthenticateIndex => "/Authenticate/OpenId",
                SimpleAuthEndPoints.ConsentIndex => "/Consent",
                SimpleAuthEndPoints.FormIndex => "/Form",
                SimpleAuthEndPoints.SendCode => "/Code",
                _ => throw new ArgumentOutOfRangeException(nameof(simpleAuthEndPoints), simpleAuthEndPoints, null)
            };


            if (!string.IsNullOrWhiteSpace(amr)
                && simpleAuthEndPoints != SimpleAuthEndPoints.ConsentIndex
                && simpleAuthEndPoints != SimpleAuthEndPoints.FormIndex)
            {
                partialUri = "/" + amr + partialUri;
            }

            return uri + partialUri;
        }

        /// <summary>
        /// Get the correct authorization request.
        /// 1. The request parameter can contains a self-contained JWT token which contains the claims of the authorization request.
        /// 2. The request_uri can be used to download the JWT token and constructs the authorization request from it.
        /// </summary>
        /// <param name="authorizationRequest"></param>
        /// <param name="cancellationToken">The cancellation token for the callback.</param>
        /// <returns></returns>
        private async Task<AuthorizationRequest> ResolveAuthorizationRequest(
            AuthorizationRequest authorizationRequest,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(authorizationRequest.request))
            {
                var result = await GetAuthorizationRequestFromJwt(
                        authorizationRequest.request,
                        authorizationRequest.client_id,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (result == null)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidRequest,
                        ErrorDescriptions.TheRequestParameterIsNotCorrect,
                        authorizationRequest.state);
                }

                return result;
            }

            if (authorizationRequest.request_uri != null)
            {
                if (!authorizationRequest.request_uri.IsAbsoluteUri)
                {
                    var httpResult = await _httpClient.GetAsync(authorizationRequest.request_uri, cancellationToken)
                            .ConfigureAwait(false);
                    if (!httpResult.IsSuccessStatusCode)
                    {
                        throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InvalidRequest,
                            ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                            authorizationRequest.state);
                    }

                    var token = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = await GetAuthorizationRequestFromJwt(
                            token,
                            authorizationRequest.client_id,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (result == null)
                    {
                        throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InvalidRequest,
                            ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                            authorizationRequest.state);
                    }

                    return result;
                }

                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRequestUriParameterIsNotWellFormed,
                    authorizationRequest.state);
            }

            return authorizationRequest;
        }
    }
}
