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

using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Common
{
    using Api.Authorization;
    using Extensions;
    using Helpers;
    using JwtToken;
    using Logging;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class GenerateAuthorizationResponse : IGenerateAuthorizationResponse
    {
        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly ITokenStore _tokenStore;
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IConsentHelper _consentHelper;
        private readonly IAuthorizationFlowHelper _authorizationFlowHelper;
        private readonly IClientStore _clientStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GenerateAuthorizationResponse(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IParameterParserHelper parameterParserHelper,
            IJwtGenerator jwtGenerator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IConsentHelper consentHelper,
            IEventPublisher eventPublisher,
            IAuthorizationFlowHelper authorizationFlowHelper,
            IClientStore clientStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _authorizationCodeStore = authorizationCodeStore;
            _tokenStore = tokenStore;
            _parameterParserHelper = parameterParserHelper;
            _jwtGenerator = jwtGenerator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _consentHelper = consentHelper;
            _eventPublisher = eventPublisher;
            _authorizationFlowHelper = authorizationFlowHelper;
            _clientStore = clientStore;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task Generate(EndpointResult endpointResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName)
        {
            if (endpointResult?.RedirectInstruction == null)
            {
                throw new ArgumentNullException(nameof(endpointResult));
            }
;
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var newAccessTokenGranted = false;
            var allowedTokenScopes = string.Empty;
            GrantedToken grantedToken = null;
            var newAuthorizationCodeGranted = false;
            AuthorizationCode authorizationCode = null;
            var responses = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
            var idTokenPayload = await GenerateIdTokenPayload(claimsPrincipal, authorizationParameter, issuerName).ConfigureAwait(false);
            var userInformationPayload = await GenerateUserInformationPayload(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            if (responses.Contains(ResponseTypeNames.Token))
            {
                // 1. Generate an access token.
                if (!string.IsNullOrWhiteSpace(authorizationParameter.Scope))
                {
                    allowedTokenScopes = string.Join(" ", _parameterParserHelper.ParseScopes(authorizationParameter.Scope));
                }

                grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId,
                    userInformationPayload, idTokenPayload).ConfigureAwait(false);
                if (grantedToken == null)
                {
                    grantedToken = await _grantedTokenGeneratorHelper.GenerateToken(
                            client,
                            allowedTokenScopes,
                            issuerName,
                            null,
                            userInformationPayload,
                            idTokenPayload)
                        .ConfigureAwait(false);
                    newAccessTokenGranted = true;
                }

                endpointResult.RedirectInstruction.AddParameter(CoreConstants.StandardAuthorizationResponseNames.AccessTokenName,
                    grantedToken.AccessToken);
            }

            if (responses.Contains(ResponseTypeNames.Code)) // 2. Generate an authorization code.
            {
                var subject = claimsPrincipal.GetSubject();
                var assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter).ConfigureAwait(false);
                if (assignedConsent != null)
                {
                    // Insert a temporary authorization code
                    // It will be used later to retrieve tha id_token or an access token.
                    authorizationCode = new AuthorizationCode
                    {
                        Code = Id.Create(),
                        RedirectUri = authorizationParameter.RedirectUrl,
                        CreateDateTime = DateTime.UtcNow,
                        ClientId = authorizationParameter.ClientId,
                        Scopes = authorizationParameter.Scope,
                        IdTokenPayload = idTokenPayload,
                        UserInfoPayLoad = userInformationPayload
                    };

                    newAuthorizationCodeGranted = true;
                    endpointResult.RedirectInstruction.AddParameter(
                        CoreConstants.StandardAuthorizationResponseNames.AuthorizationCodeName,
                        authorizationCode.Code);
                }
            }

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(
                idTokenPayload,
                authorizationCode == null ? string.Empty : authorizationCode.Code,
                grantedToken == null ? string.Empty : grantedToken.AccessToken, client);

            if (newAccessTokenGranted) // 3. Insert the stateful access token into the DB OR insert the access token into the caching.
            {
                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new AccessToClientGranted(
                            Id.Create(),
                            authorizationParameter.ClientId,
                            grantedToken.AccessToken,
                            allowedTokenScopes,
                            DateTime.UtcNow))
                    .ConfigureAwait(false);
            }

            if (newAuthorizationCodeGranted) // 4. Insert the authorization code into the caching.
            {
                if (client.RequirePkce)
                {
                    authorizationCode.CodeChallenge = authorizationParameter.CodeChallenge;
                    authorizationCode.CodeChallengeMethod = authorizationParameter.CodeChallengeMethod;
                }

                await _authorizationCodeStore.AddAuthorizationCode(authorizationCode).ConfigureAwait(false);
                await _eventPublisher.Publish(
                    new AuthorizationCodeGranted(
                        authorizationParameter.ClientId,
                        authorizationCode.Code,
                        authorizationParameter.Scope)).ConfigureAwait(false);
            }

            if (responses.Contains(ResponseTypeNames.IdToken))
            {
                var idToken = await GenerateIdToken(idTokenPayload, authorizationParameter).ConfigureAwait(false);
                endpointResult.RedirectInstruction.AddParameter(CoreConstants.StandardAuthorizationResponseNames.IdTokenName, idToken);
            }

            if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
            {
                endpointResult.RedirectInstruction.AddParameter(CoreConstants.StandardAuthorizationResponseNames.StateName, authorizationParameter.State);
            }

            var sessionState = GetSessionState(authorizationParameter.ClientId, authorizationParameter.OriginUrl, authorizationParameter.SessionId);
            if (sessionState != null)
            {
                endpointResult.RedirectInstruction.AddParameter(CoreConstants.StandardAuthorizationResponseNames.SessionState, sessionState);
            }

            if (authorizationParameter.ResponseMode == ResponseMode.form_post)
            {
                endpointResult.Type = TypeActionResult.RedirectToAction;
                endpointResult.RedirectInstruction.Action = SimpleAuthEndPoints.FormIndex;
                endpointResult.RedirectInstruction.AddParameter("redirect_uri", authorizationParameter.RedirectUrl.AbsoluteUri);
            }

            // Set the response mode
            if (endpointResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = _authorizationFlowHelper.GetAuthorizationFlow(
                        responseTypes,
                        authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                endpointResult.RedirectInstruction.ResponseMode = responseMode;
            }
        }

        private string GetSessionState(string clientId, string originUrl, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(originUrl))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            var salt = Id.Create();
            var s = $"{clientId}{originUrl}{sessionId}{salt}";
            var hex = s.ToSha256Hash();
            //var bytes = Encoding.UTF8.GetBytes(s);
            //byte[] hash;
            //using (var sha = SHA256.CreateJwk())
            //{
            //    hash = sha.ComputeHash(bytes);
            //}

            //var hex = ToHexString(hash);
            return hex.Base64Encode() + "==." + salt;
        }

        /// <summary>
        /// Generate the JWS payload for identity token.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="jwsPayload"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<string> GenerateIdToken(
            JwtPayload jwsPayload,
            AuthorizationParameter authorizationParameter)
        {
            return await _clientStore.GenerateIdTokenAsync(authorizationParameter.ClientId, jwsPayload)
                .ConfigureAwait(false);
        }

        private async Task<JwtPayload> GenerateIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            string issuerName)
        {
            JwtPayload jwsPayload;
            if (authorizationParameter.Claims != null &&
                authorizationParameter.Claims.IsAnyIdentityTokenClaimParameter())
            {
                jwsPayload = await _jwtGenerator.GenerateFilteredIdTokenPayloadAsync(claimsPrincipal, authorizationParameter, authorizationParameter.Claims.IdToken, issuerName).ConfigureAwait(false);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateIdTokenPayloadForScopesAsync(claimsPrincipal, authorizationParameter, issuerName).ConfigureAwait(false);
            }

            return jwsPayload;
        }

        /// <summary>
        /// Generate the JWS payload for user information endpoint.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="authorizationParameter"></param>
        /// <returns></returns>
        private async Task<JwtPayload> GenerateUserInformationPayload(ClaimsPrincipal claimsPrincipal, AuthorizationParameter authorizationParameter)
        {
            JwtPayload jwsPayload;
            if (authorizationParameter.Claims != null &&
                authorizationParameter.Claims.IsAnyUserInfoClaimParameter())
            {
                jwsPayload = _jwtGenerator.GenerateFilteredUserInfoPayload(
                    authorizationParameter.Claims.UserInfo,
                    claimsPrincipal,
                    authorizationParameter);
            }
            else
            {
                jwsPayload = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            }

            return jwsPayload;
        }

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
