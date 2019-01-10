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

namespace SimpleAuth.Api.Token
{
    using Actions;
    using Authenticate;
    using Errors;
    using Exceptions;
    using Helpers;
    using Logging;
    using Parameters;
    using Shared;
    using Shared.Events.OAuth;
    using Shared.Models;
    using System;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Validators;

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;
        private readonly IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
        private readonly IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;
        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly ScopeValidator _scopeValidator;
        private readonly IRevokeTokenAction _revokeTokenAction;
        private readonly IGrantedTokenHelper _grantedTokenHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenStore _tokenStore;

        public TokenActions(
            IGetTokenByResourceOwnerCredentialsGrantTypeAction getTokenByResourceOwnerCredentialsGrantType,
            IGetTokenByAuthorizationCodeGrantTypeAction getTokenByAuthorizationCodeGrantTypeAction,
            IGetTokenByRefreshTokenGrantTypeAction getTokenByRefreshTokenGrantTypeAction,
            IClientCredentialsGrantTypeParameterValidator clientCredentialsGrantTypeParameterValidator,
            IAuthenticateClient authenticateClient,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IRevokeTokenAction revokeTokenAction,
            IEventPublisher eventPublisher,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
            _getTokenByAuthorizationCodeGrantTypeAction = getTokenByAuthorizationCodeGrantTypeAction;
            _getTokenByRefreshTokenGrantTypeAction = getTokenByRefreshTokenGrantTypeAction;
            _authenticateClient = authenticateClient;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = new ScopeValidator();
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _revokeTokenAction = revokeTokenAction;
            _eventPublisher = eventPublisher;
            _tokenStore = tokenStore;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task<GrantedToken> GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            var processId = Id.Create();

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.UserName))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.UserName));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Password))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.PasswordName));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.ScopeName));
            }

            var result = await _getTokenByResourceOwnerCredentialsGrantType.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName)
                .ConfigureAwait(false);
            //var accessToken = result != null ? result.AccessToken : string.Empty;
            //var identityToken = result != null ? result.IdToken : string.Empty;
            //_oauthEventSource.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken);
            await _eventPublisher.Publish(
                new TokenGranted(Id.Create(), processId, result.AccessToken, DateTime.UtcNow)).ConfigureAwait(false);
            return result;
        }

        public async Task<GrantedToken> GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var processId = Id.Create();

            Validate(authorizationCodeGrantTypeParameter);
            var result = await _getTokenByAuthorizationCodeGrantTypeAction
                .Execute(authorizationCodeGrantTypeParameter, authenticationHeaderValue, certificate, issuerName)
                .ConfigureAwait(false);

            await _eventPublisher.Publish(
                    new TokenGranted(Id.Create(),
                        processId,
                        result.AccessToken,
                        DateTime.UtcNow))
                .ConfigureAwait(false);
            return result;
        }

        public async Task<GrantedToken> GetTokenByRefreshTokenGrantType(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            var processId = Id.Create();

            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(refreshTokenGrantTypeParameter.RefreshToken))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.RefreshToken));
            }

            var result = await _getTokenByRefreshTokenGrantTypeAction.Execute(refreshTokenGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName)
                .ConfigureAwait(false);
            //_oauthEventSource.EndGetTokenByRefreshToken(result.AccessToken, result.IdToken);
            await _eventPublisher.Publish(new TokenGranted(
                    Id.Create(),
                    processId,
                    result.AccessToken,
                    DateTime.UtcNow))
                .ConfigureAwait(false);
            return result;
        }

        public async Task<GrantedToken> GetTokenByClientCredentialsGrantType(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            var processId = Id.Create();

            var result = await GetTokenByClientCredentials(
                    clientCredentialsGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName)
                .ConfigureAwait(false);
            await _eventPublisher.Publish(
                    new TokenGranted(Id.Create(),
                        processId,
                        result.AccessToken,
                        DateTime.UtcNow))
                .ConfigureAwait(false);
            return result;
        }

        public async Task<bool> RevokeToken(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            var processId = Id.Create();

            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(revokeTokenParameter.Token))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.IntrospectionRequestNames.Token));
            }
            var result = await _revokeTokenAction.Execute(
                revokeTokenParameter,
                authenticationHeaderValue,
                certificate,
                issuerName).ConfigureAwait(false);

            await _eventPublisher.Publish(new TokenRevoked(Id.Create(), processId, DateTime.UtcNow)).ConfigureAwait(false);
            return result;
        }

        public async Task<GrantedToken> GetTokenByClientCredentials(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);

            // 1. Authenticate the client
            var instruction =
                authenticationHeaderValue.GetAuthenticateInstruction(clientCredentialsGrantTypeParameter, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.client_credentials))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantType.client_credentials));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseTypeNames.Token))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        client.ClientId,
                        ResponseTypeNames.Token));
            }

            // 3. Check scopes
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(clientCredentialsGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidScope,
                        scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 4. Generate the JWT access token on the fly.
            var grantedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId)
                .ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await _grantedTokenGeneratorHelper
                    .GenerateToken(client, allowedTokenScopes, issuerName)
                    .ConfigureAwait(false);
                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new AccessToClientGranted(
                            Id.Create(),
                            client.ClientId,
                            grantedToken.AccessToken,
                            allowedTokenScopes,
                            DateTime.UtcNow))
                    .ConfigureAwait(false);
            }

            return grantedToken;
        }

        private static void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.AuthorizationCodeName));
            }

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = parameter.RedirectUri?.IsAbsoluteUri;
            if (redirectUrlIsCorrect != true)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
            }
        }
    }
}
