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

namespace SimpleAuth.Api.Token.Actions
{
    using Authenticate;
    using JwtToken;
    using Parameters;
    using Shared;
    using Shared.Models;
    using System;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.Logging;
    using SimpleAuth.Shared.Repositories;

    internal sealed class GetTokenByRefreshTokenGrantTypeAction
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenStore _tokenStore;
        private readonly JwtGenerator _jwtGenerator;
        private readonly IClientStore _clientStore;
        private readonly AuthenticateClient _authenticateClient;

        public GetTokenByRefreshTokenGrantTypeAction(
            IEventPublisher eventPublisher,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IClientStore clientStore)
        {
            _eventPublisher = eventPublisher;
            _tokenStore = tokenStore;
            _jwtGenerator = new JwtGenerator(clientStore, scopeRepository);
            _clientStore = clientStore;
            _authenticateClient = new AuthenticateClient(clientStore);
        }

        public async Task<GrantedToken> Execute(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
                refreshTokenGrantTypeParameter,
                certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantTypes.RefreshToken))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidClient,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantTypes.RefreshToken));
            }

            // 3. Validate parameters
            var grantedToken = await ValidateParameter(refreshTokenGrantTypeParameter, cancellationToken)
                .ConfigureAwait(false);
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer);
            }

            // 4. Generate a new access token & insert it
            var generatedToken = await _clientStore.GenerateToken(
                    grantedToken.ClientId,
                    grantedToken.Scope,
                    issuerName,
                    cancellationToken,
                    grantedToken.UserInfoPayLoad,
                    grantedToken.IdTokenPayLoad)
                .ConfigureAwait(false);
            generatedToken.ParentTokenId = grantedToken.Id;
            // 5. Fill-in the idtoken
            if (generatedToken.IdTokenPayLoad != null)
            {
                _jwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad, authResult.Client);
                generatedToken.IdToken = await _clientStore.GenerateIdToken(
                        generatedToken.ClientId,
                        generatedToken.IdTokenPayLoad,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            await _tokenStore.AddToken(generatedToken, cancellationToken).ConfigureAwait(false);
            await _eventPublisher.Publish(
                    new AccessToClientGranted(
                        Id.Create(),
                        generatedToken.ClientId,
                        generatedToken.Scope,
                        DateTime.UtcNow))
                .ConfigureAwait(false);
            return generatedToken;
        }


        private async Task<GrantedToken> ValidateParameter(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
            CancellationToken cancellationToken)
        {
            var grantedToken = await _tokenStore
                .GetRefreshToken(refreshTokenGrantTypeParameter.RefreshToken, cancellationToken)
                .ConfigureAwait(false);
            if (grantedToken == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheRefreshTokenIsNotValid);
            }

            return grantedToken;
        }
    }
}
