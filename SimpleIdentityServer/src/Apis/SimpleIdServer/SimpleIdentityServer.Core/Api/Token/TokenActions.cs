﻿// Copyright 2015 Habart Thierry
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
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.OAuth.Logging;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Token
{
    using Common;
    using Errors;
    using SimpleIdentityServer.Common.Dtos.Events.OAuth;

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;
        private readonly IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
        private readonly IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;
        private readonly IGetTokenByClientCredentialsGrantTypeAction _getTokenByClientCredentialsGrantTypeAction;
        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;
        private readonly IRevokeTokenParameterValidator _revokeTokenParameterValidator;
        private readonly IRevokeTokenAction _revokeTokenAction;
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPayloadSerializer _payloadSerializer;

        public TokenActions(
            IGetTokenByResourceOwnerCredentialsGrantTypeAction getTokenByResourceOwnerCredentialsGrantType,
            IGetTokenByAuthorizationCodeGrantTypeAction getTokenByAuthorizationCodeGrantTypeAction,
            IGetTokenByRefreshTokenGrantTypeAction getTokenByRefreshTokenGrantTypeAction,
            IGetTokenByClientCredentialsGrantTypeAction getTokenByClientCredentialsGrantTypeAction,
            IClientCredentialsGrantTypeParameterValidator clientCredentialsGrantTypeParameterValidator,
            IRevokeTokenParameterValidator revokeTokenParameterValidator,
            IOAuthEventSource oauthEventSource,
            IRevokeTokenAction revokeTokenAction,
            IEventPublisher eventPublisher,
            IPayloadSerializer payloadSerializer)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
            _getTokenByAuthorizationCodeGrantTypeAction = getTokenByAuthorizationCodeGrantTypeAction;
            _getTokenByRefreshTokenGrantTypeAction = getTokenByRefreshTokenGrantTypeAction;
            _oauthEventSource = oauthEventSource;
            _getTokenByClientCredentialsGrantTypeAction = getTokenByClientCredentialsGrantTypeAction;
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _revokeTokenParameterValidator = revokeTokenParameterValidator;
            _revokeTokenAction = revokeTokenAction;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
        }

        public async Task<GrantedToken> GetTokenByResourceOwnerCredentialsGrantType(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new GrantTokenViaResourceOwnerCredentialsReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(resourceOwnerGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByResourceOwnerCredentials(resourceOwnerGrantTypeParameter.ClientId,
                    resourceOwnerGrantTypeParameter.UserName,
                    resourceOwnerGrantTypeParameter.Password);
                Validate(resourceOwnerGrantTypeParameter);
                var result = await _getTokenByResourceOwnerCredentialsGrantType.Execute(resourceOwnerGrantTypeParameter,
                    authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                var accessToken = result != null ? result.AccessToken : string.Empty;
                var identityToken = result != null ? result.IdToken : string.Empty;
                _oauthEventSource.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId,  _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch(IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }

        public async Task<GrantedToken> GetTokenByAuthorizationCodeGrantType(AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate, string issuerName)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new GrantTokenViaAuthorizationCodeReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(authorizationCodeGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByAuthorizationCode(
                    authorizationCodeGrantTypeParameter.ClientId,
                    authorizationCodeGrantTypeParameter.Code);
                Validate(authorizationCodeGrantTypeParameter);
                var result = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByAuthorizationCode(
                    result.AccessToken,
                    result.IdToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }

        public async Task<GrantedToken> GetTokenByRefreshTokenGrantType(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new GrantTokenViaRefreshTokenReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(refreshTokenGrantTypeParameter), 0));
                _oauthEventSource.StartGetTokenByRefreshToken(refreshTokenGrantTypeParameter.RefreshToken);
                // Read this RFC for more information
                if (string.IsNullOrWhiteSpace(refreshTokenGrantTypeParameter.RefreshToken))
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.RefreshToken));
                }
                var result = await _getTokenByRefreshTokenGrantTypeAction.Execute(refreshTokenGrantTypeParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByRefreshToken(result.AccessToken, result.IdToken);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }

        public async Task<GrantedToken> GetTokenByClientCredentialsGrantType(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate, string issuerName)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new GrantTokenViaClientCredentialsReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(clientCredentialsGrantTypeParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartGetTokenByClientCredentials(clientCredentialsGrantTypeParameter.Scope);
                _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);
                var result = await _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);
                _oauthEventSource.EndGetTokenByClientCredentials(
                    result.ClientId,
                    clientCredentialsGrantTypeParameter.Scope);
                _eventPublisher.Publish(new TokenGranted(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }

        public Task<bool> RevokeToken(
            RevokeTokenParameter revokeTokenParameter, 
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate, string issuerName)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new RevokeTokenReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(revokeTokenParameter, authenticationHeaderValue), authenticationHeaderValue, 0));
                _oauthEventSource.StartRevokeToken(revokeTokenParameter.Token);
                _revokeTokenParameterValidator.Validate(revokeTokenParameter);
                var result = _revokeTokenAction.Execute(revokeTokenParameter, authenticationHeaderValue, certificate, issuerName);
                _oauthEventSource.EndRevokeToken(revokeTokenParameter.Token);
                _eventPublisher.Publish(new TokenRevoked(Guid.NewGuid().ToString(), processId, 1));
                return result;
            }
            catch (IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }

        private void Validate(ResourceOwnerGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.UserName))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.UserName));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.PasswordName));
            }

            if (string.IsNullOrWhiteSpace(parameter.Scope))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.ScopeName));
            }
        }

        public void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardTokenRequestParameterNames.AuthorizationCodeName));
            }

            // With this instruction
            // The redirect_uri is considered well-formed according to the RFC-3986
            var redirectUrlIsCorrect = Uri.IsWellFormedUriString(parameter.RedirectUri, UriKind.Absolute);
            if (!redirectUrlIsCorrect)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
            }
        }
    }
}
