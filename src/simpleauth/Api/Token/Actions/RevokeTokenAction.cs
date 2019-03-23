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
    using Parameters;
    using System;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal class RevokeTokenAction
    {
        private readonly AuthenticateClient _authenticateClient;
        private readonly ITokenStore _tokenStore;

        public RevokeTokenAction(IClientStore clientStore, ITokenStore tokenStore)
        {
            _authenticateClient = new AuthenticateClient(clientStore);
            _tokenStore = tokenStore;
        }

        public async Task<bool> Execute(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            // 1. Check the client credentials
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(revokeTokenParameter, certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Retrieve the granted token & check if it exists
            var grantedToken = await _tokenStore.GetAccessToken(revokeTokenParameter.Token, cancellationToken)
                .ConfigureAwait(false);
            var isAccessToken = true;
            if (grantedToken == null)
            {
                grantedToken = await _tokenStore.GetRefreshToken(revokeTokenParameter.Token, cancellationToken)
                    .ConfigureAwait(false);
                isAccessToken = false;
            }

            if (grantedToken == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidToken, ErrorDescriptions.TheTokenDoesntExist);
            }

            // 3. Verifies whether the token was issued to the client making the revocation request
            if (grantedToken.ClientId != client.ClientId)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidToken,
                    string.Format(ErrorDescriptions.TheTokenHasNotBeenIssuedForTheGivenClientId, client.ClientId));
            }

            // 4. Invalid the granted token
            return isAccessToken
                ? await _tokenStore.RemoveAccessToken(grantedToken.AccessToken, cancellationToken).ConfigureAwait(false)
                : await _tokenStore.RemoveRefreshToken(grantedToken.RefreshToken, cancellationToken)
                    .ConfigureAwait(false);
        }
    }
}
