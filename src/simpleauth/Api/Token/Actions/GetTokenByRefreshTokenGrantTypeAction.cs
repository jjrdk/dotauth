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
    using Errors;
    using Exceptions;
    using Helpers;
    using JwtToken;
    using Logging;
    using Parameters;
    using Shared;
    using Shared.Models;
    using System;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public sealed class GetTokenByRefreshTokenGrantTypeAction
    {
        private readonly IClientHelper _clientHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly ITokenStore _tokenStore;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly AuthenticateClient _authenticateClient;

        public GetTokenByRefreshTokenGrantTypeAction(
            IClientHelper clientHelper,
            IEventPublisher eventPublisher,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            ITokenStore tokenStore,
            IJwtGenerator jwtGenerator,
            IClientStore clientStore)
        {
            _clientHelper = clientHelper;
            _eventPublisher = eventPublisher;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _tokenStore = tokenStore;
            _jwtGenerator = jwtGenerator;
            _authenticateClient = new AuthenticateClient(clientStore);
        }

        public async Task<GrantedToken> Execute(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(refreshTokenGrantTypeParameter, certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check client
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.refresh_token))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.refresh_token));
            }

            // 3. Validate parameters
            var grantedToken = await ValidateParameter(refreshTokenGrantTypeParameter).ConfigureAwait(false);
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer);
            }

            // 4. Generate a new access token & insert it
            var generatedToken = await _grantedTokenGeneratorHelper.GenerateToken(
                grantedToken.ClientId,
                grantedToken.Scope,
                issuerName,
                null,
                grantedToken.UserInfoPayLoad,
                grantedToken.IdTokenPayLoad).ConfigureAwait(false);
            generatedToken.ParentTokenId = grantedToken.Id;
            // 5. Fill-in the idtoken
            if (generatedToken.IdTokenPayLoad != null)
            {
                _jwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad, authResult.Client);
                generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(generatedToken.ClientId, generatedToken.IdTokenPayLoad).ConfigureAwait(false);
            }

            await _tokenStore.AddToken(generatedToken).ConfigureAwait(false);
            await _eventPublisher.Publish(
                new AccessToClientGranted(
                    Id.Create(),
                    generatedToken.ClientId,
                    generatedToken.AccessToken,
                    generatedToken.Scope,
                    DateTime.UtcNow)).ConfigureAwait(false);
            return generatedToken;
        }


        private async Task<GrantedToken> ValidateParameter(RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter)
        {
            var grantedToken = await _tokenStore.GetRefreshToken(refreshTokenGrantTypeParameter.RefreshToken).ConfigureAwait(false);
            if (grantedToken == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRefreshTokenIsNotValid);
            }

            return grantedToken;
        }
    }
}
