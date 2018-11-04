// Copyright 2015 Habart Thierry
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

using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Store;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    using Logging;

    public class GetTokenByAuthorizationCodeGrantTypeAction : IGetTokenByAuthorizationCodeGrantTypeAction
    {
        private class ValidationResult
        {
            public AuthorizationCode AuthCode { get; set; }
            public Client Client { get; set; }
        }

        private readonly IClientValidator _clientValidator;
        private readonly IAuthorizationCodeStore _authorizationCodeStore;
        private readonly OAuthConfigurationOptions _configurationService;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IClientHelper _clientHelper;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly ITokenStore _tokenStore;
        private readonly IGrantedTokenHelper _grantedTokenHelper;
        private readonly IJwtGenerator _jwtGenerator;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeStore authorizationCodeStore,
            OAuthConfigurationOptions configurationService,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IAuthenticateClient authenticateClient,
            IClientHelper clientHelper,
            IOAuthEventSource oauthEventSource,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper,
            IJwtGenerator jwtGenerator)
        {
            _clientValidator = clientValidator;
            _authorizationCodeStore = authorizationCodeStore;
            _configurationService = configurationService;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _authenticateClient = authenticateClient;
            _clientHelper = clientHelper;
            _oauthEventSource = oauthEventSource;
            _tokenStore = tokenStore;
            _grantedTokenHelper = grantedTokenHelper;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<GrantedToken> Execute(AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var result = await ValidateParameter(authorizationCodeGrantTypeParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
            await _authorizationCodeStore.RemoveAuthorizationCode(result.AuthCode.Code).ConfigureAwait(false); // 1. Invalidate the authorization code by removing it !
            var grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(
                result.AuthCode.Scopes,
                result.AuthCode.ClientId,
                result.AuthCode.IdTokenPayload,
                result.AuthCode.UserInfoPayLoad).ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await _grantedTokenGeneratorHelper.GenerateTokenAsync(
                        result.Client,
                        result.AuthCode.Scopes,
                        issuerName,
                        null,
                        result.AuthCode.UserInfoPayLoad,
                        result.AuthCode.IdTokenPayload)
                    .ConfigureAwait(false);
                _oauthEventSource.GrantAccessToClient(
                    result.AuthCode.ClientId,
                    grantedToken.AccessToken,
                    grantedToken.IdToken);
                // Fill-in the id-token
                if (grantedToken.IdTokenPayLoad != null)
                {
                    await _jwtGenerator.UpdatePayloadDate(grantedToken.IdTokenPayLoad).ConfigureAwait(false);
                    grantedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(result.Client, grantedToken.IdTokenPayLoad).ConfigureAwait(false);
                }

                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
            }

            return grantedToken;
        }

        /// <summary>
        /// Check the parameters based on the RFC : http://openid.net/specs/openid-connect-core-1_0.html#TokenRequestValidation
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <returns></returns>
        private async Task<ValidationResult> ValidateParameter(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            // 1. Authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(authorizationCodeGrantTypeParameter, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                _oauthEventSource.Info(authResult.ErrorMessage);
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check the client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.authorization_code))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.authorization_code));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseType.code))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.ClientId, ResponseType.code));
            }

            var authorizationCode = await _authorizationCodeStore.GetAuthorizationCode(authorizationCodeGrantTypeParameter.Code).ConfigureAwait(false);
            // 2. Check if the authorization code is valid
            if (authorizationCode == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
            }

            // 3. Check PKCE
            if (!_clientValidator.CheckPkce(client, authorizationCodeGrantTypeParameter.CodeVerifier, authorizationCode))
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheCodeVerifierIsNotCorrect);
            }

            // 4. Ensure the authorization code was issued to the authenticated client.
            var authorizationClientId = authorizationCode.ClientId;
            if (authorizationClientId != client.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        client.ClientId));
            }

            if (authorizationCode.RedirectUri != authorizationCodeGrantTypeParameter.RedirectUri)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
            }

            // 5. Ensure the authorization code is still valid.
            var authCodeValidity = _configurationService.AuthorizationCodeValidityPeriod;
            var expirationDateTime = authorizationCode.CreateDateTime.Add(authCodeValidity);
            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime > expirationDateTime)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsObsolete);
            }

            // Ensure that the redirect_uri parameter value is identical to the redirect_uri parameter value.
            var redirectionUrl = _clientValidator.GetRedirectionUrls(client, authorizationCodeGrantTypeParameter.RedirectUri);
            if (!redirectionUrl.Any())
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, authorizationCodeGrantTypeParameter.RedirectUri));
            }

            return new ValidationResult
            {
                Client = client,
                AuthCode = authorizationCode
            };
        }
    }
}
