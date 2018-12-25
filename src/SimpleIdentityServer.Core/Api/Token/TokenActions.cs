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

using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token
{
    using Authenticate;
    using Errors;
    using Helpers;
    using Logging;
    using SimpleAuth.Jwt;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Events.OAuth;
    using SimpleAuth.Shared.Models;

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;
        private readonly IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
        private readonly IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;
        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly IRevokeTokenParameterValidator _revokeTokenParameterValidator;
        private readonly IScopeValidator _scopeValidator;
        private readonly IRevokeTokenAction _revokeTokenAction;
        private readonly IOAuthEventSource _oauthEventSource;
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
            IRevokeTokenParameterValidator revokeTokenParameterValidator,
            IScopeValidator scopeValidator,
            IOAuthEventSource oauthEventSource,
            IRevokeTokenAction revokeTokenAction,
            IEventPublisher eventPublisher,
            ITokenStore tokenStore,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
            _getTokenByAuthorizationCodeGrantTypeAction = getTokenByAuthorizationCodeGrantTypeAction;
            _getTokenByRefreshTokenGrantTypeAction = getTokenByRefreshTokenGrantTypeAction;
            _oauthEventSource = oauthEventSource;
            _authenticateClient = authenticateClient;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = scopeValidator;
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _revokeTokenParameterValidator = revokeTokenParameterValidator;
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

            var processId = Guid.NewGuid().ToString();
            try
            {
                //_eventPublisher.Publish(new GrantTokenViaResourceOwnerCredentialsReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(resourceOwnerGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByResourceOwnerCredentials(resourceOwnerGrantTypeParameter.ClientId,
                    resourceOwnerGrantTypeParameter.UserName,
                    resourceOwnerGrantTypeParameter.Password);
                if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.UserName))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardTokenRequestParameterNames.UserName));
                }

                if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Password))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardTokenRequestParameterNames.PasswordName));
                }

                if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardTokenRequestParameterNames.ScopeName));
                }

                var result = await _getTokenByResourceOwnerCredentialsGrantType.Execute(resourceOwnerGrantTypeParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName)
                    .ConfigureAwait(false);
                var accessToken = result != null ? result.AccessToken : string.Empty;
                var identityToken = result != null ? result.IdToken : string.Empty;
                _oauthEventSource.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, result.AccessToken, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(),
                    processId,
                    ex.Code,
                    ex.Message,
                    1));
                throw;
            }
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

            var processId = Guid.NewGuid().ToString();
            try
            {
                //_eventPublisher.Publish(new GrantTokenViaAuthorizationCodeReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(authorizationCodeGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByAuthorizationCode(
                    authorizationCodeGrantTypeParameter.ClientId,
                    authorizationCodeGrantTypeParameter.Code);
                Validate(authorizationCodeGrantTypeParameter);
                var result = await _getTokenByAuthorizationCodeGrantTypeAction
                    .Execute(authorizationCodeGrantTypeParameter, authenticationHeaderValue, certificate, issuerName)
                    .ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByAuthorizationCode(
                    result.AccessToken,
                    result.IdToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, result.AccessToken, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(),
                    processId,
                    ex.Code,
                    ex.Message,
                    1));
                throw;
            }
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

            var processId = Guid.NewGuid().ToString();
            try
            {
                //_eventPublisher.Publish(new GrantTokenViaRefreshTokenReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(refreshTokenGrantTypeParameter), 0));
                _oauthEventSource.StartGetTokenByRefreshToken(refreshTokenGrantTypeParameter.RefreshToken);
                // Read this RFC for more information
                if (string.IsNullOrWhiteSpace(refreshTokenGrantTypeParameter.RefreshToken))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardTokenRequestParameterNames.RefreshToken));
                }

                var result = await _getTokenByRefreshTokenGrantTypeAction.Execute(refreshTokenGrantTypeParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName)
                    .ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByRefreshToken(result.AccessToken, result.IdToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, result.AccessToken, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(),
                    processId,
                    ex.Code,
                    ex.Message,
                    1));
                throw;
            }
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

            var processId = Guid.NewGuid().ToString();
            try
            {
                //_eventPublisher.Publish(new GrantTokenViaClientCredentialsReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(clientCredentialsGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByClientCredentials(clientCredentialsGrantTypeParameter.Scope);
                // _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);
                var result = await GetTokenByClientCredentials(
                        clientCredentialsGrantTypeParameter,
                        authenticationHeaderValue,
                        certificate,
                        issuerName)
                    .ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByClientCredentials(
                    result.ClientId,
                    clientCredentialsGrantTypeParameter.Scope);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, result.AccessToken, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(),
                    processId,
                    ex.Code,
                    ex.Message,
                    1));
                throw;
            }
        }

        public Task<bool> RevokeToken(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                //_eventPublisher.Publish(new RevokeTokenReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(revokeTokenParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartRevokeToken(revokeTokenParameter.Token);
                _revokeTokenParameterValidator.Validate(revokeTokenParameter);
                var result = _revokeTokenAction.Execute(revokeTokenParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName);
                _oauthEventSource.EndRevokeToken(revokeTokenParameter.Token);
                _eventPublisher.Publish(new TokenRevoked(Guid.NewGuid().ToString(), processId, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(),
                    processId,
                    ex.Code,
                    ex.Message,
                    1));
                throw;
            }
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
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.client_credentials))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantType.client_credentials));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseType.token))
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        client.ClientId,
                        ResponseType.token));
            }

            // 3. Check scopes
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(clientCredentialsGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new IdentityServerException(
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
                    .GenerateTokenAsync(client, allowedTokenScopes, issuerName, null)
                    .ConfigureAwait(false);
                await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
                _oauthEventSource.GrantAccessToClient(client.ClientId, grantedToken.AccessToken, allowedTokenScopes);
            }

            return grantedToken;
        }

        private static void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter,
                        CoreConstants.StandardTokenRequestParameterNames.AuthorizationCodeName));
            }

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = parameter.RedirectUri?.IsAbsoluteUri;
            if (redirectUrlIsCorrect != true)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
            }
        }
    }
}
