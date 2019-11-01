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
    using Parameters;
    using Shared;
    using Shared.Events.OAuth;
    using Shared.Models;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TokenActions
    {
        private readonly GetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;
        private readonly GetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
        private readonly GetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;
        private readonly AuthenticateClient _authenticateClient;
        private readonly RevokeTokenAction _revokeTokenAction;
        private readonly IJwksStore _jwksStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenStore _tokenStore;

        public TokenActions(
            RuntimeSettings simpleAuthOptions,
            IAuthorizationCodeStore authorizationCodeStore,
            IClientStore clientStore,
            IScopeRepository scopeRepository,
            IJwksStore jwksStore,
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
            IEventPublisher eventPublisher,
            ITokenStore tokenStore)
        {
            _getTokenByResourceOwnerCredentialsGrantType = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
                clientStore,
                scopeRepository,
                tokenStore,
                jwksStore,
                resourceOwnerServices,
                eventPublisher);
            _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
                authorizationCodeStore,
                simpleAuthOptions,
                clientStore,
                eventPublisher,
                tokenStore,
                scopeRepository,
                jwksStore);
            _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
                simpleAuthOptions,
                eventPublisher,
                tokenStore,
                scopeRepository,
                jwksStore,
                resourceOwnerRepository,
                clientStore);
            _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
            _revokeTokenAction = new RevokeTokenAction(clientStore, tokenStore, jwksStore);
            _jwksStore = jwksStore;
            _eventPublisher = eventPublisher;
            _tokenStore = tokenStore;
        }

        public Task<GrantedToken> GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.UserName))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, StandardTokenRequestParameterNames.UserName));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Password))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, StandardTokenRequestParameterNames.PasswordName));
            }

            if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, StandardTokenRequestParameterNames.ScopeName));
            }

            return _getTokenByResourceOwnerCredentialsGrantType.Execute(
                resourceOwnerGrantTypeParameter,
                authenticationHeaderValue,
                certificate,
                issuerName,
                cancellationToken);
        }

        public Task<GrantedToken> GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            Validate(authorizationCodeGrantTypeParameter);
            return _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                    authorizationCodeGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName,
                    cancellationToken);
        }

        public Task<GrantedToken> GetTokenByRefreshTokenGrantType(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(refreshTokenGrantTypeParameter.RefreshToken))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, StandardTokenRequestParameterNames.RefreshToken));
            }

            return _getTokenByRefreshTokenGrantTypeAction.Execute(
                    refreshTokenGrantTypeParameter,
                    authenticationHeaderValue,
                    certificate,
                    issuerName,
                    cancellationToken);
        }

        public async Task<GrantedToken> GetTokenByClientCredentialsGrantType(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, StandardTokenRequestParameterNames.ScopeName));
            }

            // 1. Authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
                clientCredentialsGrantTypeParameter,
                certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || client.GrantTypes.All(x => x != GrantTypes.ClientCredentials))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClient,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantTypes.ClientCredentials));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseTypeNames.Token))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClient,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                        client.ClientId,
                        ResponseTypeNames.Token));
            }

            // 3. Check scopes
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
            {
                var scopeValidation = clientCredentialsGrantTypeParameter.Scope.Check(client);
                if (!scopeValidation.IsValid)
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope, scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 4. Generate the JWT access token on the fly.
            var grantedToken = await _tokenStore
                .GetValidGrantedToken(allowedTokenScopes, client.ClientId, cancellationToken)
                .ConfigureAwait(false);
            if (grantedToken == null)
            {
                grantedToken = await client.GenerateToken(
                        _jwksStore,
                        allowedTokenScopes,
                        issuerName,
                        cancellationToken: cancellationToken,
                        additionalClaims: client.Claims.Where(
                                c => client.UserClaimsToIncludeInAuthToken?.Any(r => r.IsMatch(c.Type)) == true)
                            .ToArray())
                    .ConfigureAwait(false);
                await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new TokenGranted(
                            Id.Create(),
                            grantedToken?.UserInfoPayLoad?.Sub,
                            grantedToken?.ClientId,
                            grantedToken?.Scope,
                            GrantTypes.ClientCredentials,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
            }

            return grantedToken;
        }

        public async Task<bool> RevokeToken(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
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
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);

            await _eventPublisher
                .Publish(new TokenRevoked(Id.Create(), revokeTokenParameter.Token, DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            return result;
        }

        private static void Validate(AuthorizationCodeGrantTypeParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Code))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(
                        ErrorDescriptions.MissingParameter,
                        StandardTokenRequestParameterNames.AuthorizationCodeName));
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
