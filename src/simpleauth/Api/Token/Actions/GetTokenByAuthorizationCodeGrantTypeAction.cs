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

namespace SimpleAuth.Api.Token.Actions
{
    using Authenticate;
    using JwtToken;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.Logging;
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    internal class GetTokenByAuthorizationCodeGrantTypeAction
    {
        private class ValidationResult
        {
            public AuthorizationCode AuthCode { get; set; }
            public Client Client { get; set; }
        }

        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly RuntimeSettings _configurationService;
        private readonly AuthenticateClient _authenticateClient;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenStore _tokenStore;
        private readonly IJwksStore _jwksStore;
        private readonly JwtGenerator _jwtGenerator;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IAuthorizationCodeStore authorizationCodeStore,
            RuntimeSettings configurationService,
            IClientStore clientStore,
            IEventPublisher eventPublisher,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IJwksStore jwksStore)
        {
            _authorizationCodeStore = authorizationCodeStore;
            _configurationService = configurationService;
            _authenticateClient = new AuthenticateClient(clientStore);
            _eventPublisher = eventPublisher;
            _tokenStore = tokenStore;
            _jwksStore = jwksStore;
            _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore);
        }

        public async Task<GrantedToken> Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var result = await ValidateParameter(
                    authorizationCodeGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            await _authorizationCodeStore.Remove(result.AuthCode.Code, cancellationToken)
                .ConfigureAwait(false); // 1. Invalidate the authorization code by removing it !
            var grantedToken = await _tokenStore.GetValidGrantedToken(
                    result.AuthCode.Scopes,
                    result.AuthCode.ClientId,
                    cancellationToken,
                    idTokenJwsPayload: result.AuthCode.IdTokenPayload,
                    userInfoJwsPayload: result.AuthCode.UserInfoPayLoad)
                .ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await result.Client.GenerateToken(
                        _jwksStore,
                        result.AuthCode.Scopes,
                        issuerName,
                        result.AuthCode.UserInfoPayLoad,
                        result.AuthCode.IdTokenPayload,
                        cancellationToken,
                        result.AuthCode.IdTokenPayload?.Claims.Where(
                                c => _configurationService.UserClaimsToIncludeInAuthToken.Any(r => r.IsMatch(c.Type)))
                            .ToArray())
                    .ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new AccessToClientGranted(
                            Id.Create(),
                            result.AuthCode.ClientId,
                            grantedToken.IdToken,
                            DateTime.UtcNow))
                    .ConfigureAwait(false);
                // Fill-in the id-token
                if (grantedToken.IdTokenPayLoad != null)
                {
                    _jwtGenerator.UpdatePayloadDate(grantedToken.IdTokenPayLoad, result.Client?.TokenLifetime);
                    grantedToken.IdToken = result.Client.GenerateIdToken(grantedToken.IdTokenPayLoad);
                }

                await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);
            }

            return grantedToken;
        }

        /// <summary>
        /// Check the parameters based on the RFC : http://openid.net/specs/openid-connect-core-1_0.html#TokenRequestValidation
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <param name="certificate"></param>
        /// <param name="issuerName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<ValidationResult> ValidateParameter(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            // 1. Authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
                authorizationCodeGrantTypeParameter,
                certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check the client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantTypes.AuthorizationCode))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClient,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantTypes.AuthorizationCode));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseTypeNames.Code))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClient,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        client.ClientId,
                        ResponseTypeNames.Code));
            }

            var authorizationCode = await _authorizationCodeStore
                .Get(authorizationCodeGrantTypeParameter.Code, cancellationToken)
                .ConfigureAwait(false);
            // 2. Check if the authorization code is valid
            if (authorizationCode == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
            }

            // 3. Check PKCE
            if (!client.CheckPkce(authorizationCodeGrantTypeParameter.CodeVerifier, authorizationCode))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheCodeVerifierIsNotCorrect);
            }

            // 4. Ensure the authorization code was issued to the authenticated client.
            var authorizationClientId = authorizationCode.ClientId;
            if (authorizationClientId != client.ClientId)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    string.Format(
                        ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        client.ClientId));
            }

            if (authorizationCode.RedirectUri != authorizationCodeGrantTypeParameter.RedirectUri)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
            }

            // 5. Ensure the authorization code is still valid.
            var authCodeValidity = _configurationService.AuthorizationCodeValidityPeriod;
            var expirationDateTime = authorizationCode.CreateDateTime.Add(authCodeValidity);
            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime > expirationDateTime)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsObsolete);
            }

            // Ensure that the redirect_uri parameter value is identical to the redirect_uri parameter value.
            var redirectionUrl = client.GetRedirectionUrls(authorizationCodeGrantTypeParameter.RedirectUri);
            if (!redirectionUrl.Any())
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    string.Format(
                        ErrorDescriptions.RedirectUrlIsNotValid,
                        authorizationCodeGrantTypeParameter.RedirectUri));
            }

            return new ValidationResult { Client = client, AuthCode = authorizationCode };
        }
    }
}
