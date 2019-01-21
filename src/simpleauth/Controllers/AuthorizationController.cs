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
    using Common;
    using Errors;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Parameters;
    using Parsers;
    using Results;
    using Shared;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;
    using Shared.Serializers;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [Route(CoreConstants.EndPoints.Authorization)]
    public class AuthorizationController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly AuthorizationActions _authorizationActions;
        private readonly IDataProtector _dataProtector;
        private readonly IActionResultParser _actionResultParser;
        private readonly IAuthenticationService _authenticationService;
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();

        public AuthorizationController(
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IEventPublisher eventPublisher,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            IClientStore clientStore,
            IConsentRepository consentRepository,
            IDataProtectionProvider dataProtectionProvider,
            IActionResultParser actionResultParser,
            IAuthenticationService authenticationService)
        {
            _clientStore = clientStore;
            _authorizationActions = new AuthorizationActions(
                generateAuthorizationResponse,
                clientStore,
                consentRepository,
                authorizationFlowHelper,
                eventPublisher,
                resourceOwnerAuthenticateHelper);
            _dataProtector = dataProtectionProvider.CreateProtector("Request");
            _actionResultParser = actionResultParser;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = Request.Query;
            if (query == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var originUrl = this.GetOriginUrl();
            var sessionId = GetSessionId();
            var serializer = new ParamSerializer();
            var authorizationRequest =
                serializer.Deserialize<AuthorizationRequest>(query.Select(x =>
                    new KeyValuePair<string, string[]>(x.Key, x.Value)));
            authorizationRequest = await ResolveAuthorizationRequest(authorizationRequest).ConfigureAwait(false);
            authorizationRequest.OriginUrl = originUrl;
            authorizationRequest.SessionId = sessionId;
            var authenticatedUser = await _authenticationService
                .GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName)
                .ConfigureAwait(false);
            var parameter = authorizationRequest.ToParameter();
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var actionResult = await _authorizationActions.GetAuthorization(parameter, authenticatedUser, issuerName)
                .ConfigureAwait(false);

            switch (actionResult.Type)
            {
                case TypeActionResult.RedirectToCallBackUrl:
                    {
                        //var redirectUrl = new Uri();
                        return this.CreateRedirectHttpTokenResponse(
                            authorizationRequest.RedirectUri,
                            _actionResultParser.GetRedirectionParameters(actionResult),
                            actionResult.RedirectInstruction.ResponseMode);
                    }
                case TypeActionResult.RedirectToAction:
                    {
                        if (actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.AuthenticateIndex ||
                            actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.ConsentIndex)
                        {
                            // Force the resource owner to be reauthenticated
                            if (actionResult.RedirectInstruction.Action == SimpleAuthEndPoints.AuthenticateIndex)
                            {
                                authorizationRequest.Prompt = Enum.GetName(typeof(PromptParameter), PromptParameter.login);
                            }

                            // Set the process id into the request.
                            if (!string.IsNullOrWhiteSpace(actionResult.ProcessId))
                            {
                                authorizationRequest.ProcessId = actionResult.ProcessId;
                            }

                            // Add the encoded request into the query string
                            var encryptedRequest = _dataProtector.Protect(authorizationRequest);
                            actionResult.RedirectInstruction.AddParameter(
                                CoreConstants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                                encryptedRequest);
                        }

                        var url = GetRedirectionUrl(Request, actionResult.Amr, actionResult.RedirectInstruction.Action);
                        var uri = new Uri(url);
                        var redirectionUrl =
                            uri.AddParametersInQuery(_actionResultParser.GetRedirectionParameters(actionResult));
                        return new RedirectResult(redirectionUrl.AbsoluteUri);
                    }
                //case TypeActionResult.Output:
                //case TypeActionResult.None:
                default:
                    return null;
            }
        }

        private string GetSessionId()
        {
            return !Request.Cookies.ContainsKey(CoreConstants.SESSION_ID) ? Id.Create() : Request.Cookies[CoreConstants.SESSION_ID];
        }

        private async Task<AuthorizationRequest> GetAuthorizationRequestFromJwt(string token, string clientId)
        {
            var client = await _clientStore.GetById(clientId).ConfigureAwait(false);
            _handler.ValidateToken(token, client.CreateValidationParameters(), out var securityToken);

            return (securityToken as JwtSecurityToken)?.Payload?.ToAuthorizationRequest();
        }

        private static string GetRedirectionUrl(Microsoft.AspNetCore.Http.HttpRequest request,
            string amr,
            SimpleAuthEndPoints simpleAuthEndPoints)
        {
            var uri = request.GetAbsoluteUriWithVirtualPath();
            var partialUri = HostConstants.MappingEndPointToPartialUrl[simpleAuthEndPoints];
            if (!string.IsNullOrWhiteSpace(amr) &&
                simpleAuthEndPoints != SimpleAuthEndPoints.ConsentIndex &&
                simpleAuthEndPoints != SimpleAuthEndPoints.FormIndex)
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
        /// <returns></returns>
        private async Task<AuthorizationRequest> ResolveAuthorizationRequest(AuthorizationRequest authorizationRequest)
        {
            if (!string.IsNullOrWhiteSpace(authorizationRequest.Request))
            {
                var result =
                    await GetAuthorizationRequestFromJwt(authorizationRequest.Request, authorizationRequest.ClientId)
                        .ConfigureAwait(false);
                if (result == null)
                {
                    throw new SimpleAuthExceptionWithState(ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheRequestParameterIsNotCorrect,
                        authorizationRequest.State);
                }

                return result;
            }

            if (!string.IsNullOrWhiteSpace(authorizationRequest.RequestUri))
            {
                if (Uri.TryCreate(authorizationRequest.RequestUri, UriKind.Absolute, out var uri))
                {
                    try
                    {
                        var httpClient = new HttpClient
                        {
                            BaseAddress = uri
                        };

                        var httpResult = await httpClient.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
                        httpResult.EnsureSuccessStatusCode();
                        var request = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result = await GetAuthorizationRequestFromJwt(request, authorizationRequest.ClientId)
                            .ConfigureAwait(false);
                        if (result == null)
                        {
                            throw new SimpleAuthExceptionWithState(
                                ErrorCodes.InvalidRequestCode,
                                ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                                authorizationRequest.State);
                        }

                        return result;
                    }
                    catch (Exception)
                    {
                        throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InvalidRequestCode,
                            ErrorDescriptions.TheRequestDownloadedFromRequestUriIsNotValid,
                            authorizationRequest.State);
                    }
                }

                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    ErrorDescriptions.TheRequestUriParameterIsNotWellFormed,
                    authorizationRequest.State);
            }

            return authorizationRequest;
        }

        /// <summary>
        /// Build the JSON error message.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}