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
    using System.Net;
    using Authenticate;
    using Parameters;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;

    internal class RevokeTokenAction
    {
        private readonly AuthenticateClient _authenticateClient;
        private readonly ITokenStore _tokenStore;
        private readonly ILogger _logger;

        public RevokeTokenAction(IClientStore clientStore, ITokenStore tokenStore, IJwksStore jwksStore, ILogger logger)
        {
            _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
            _tokenStore = tokenStore;
            _logger = logger;
        }

        public async Task<Option> Execute(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue? authenticationHeaderValue,
            X509Certificate2? certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var refreshToken = revokeTokenParameter.Token;
            if (refreshToken == null)
            {
                _logger.LogError(Strings.TheRefreshTokenIsNotValid);
                return new ErrorDetails
                {
                    Detail = Strings.TheRefreshTokenIsNotValid,
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidToken
                };
            }
            // 1. Check the client credentials
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(revokeTokenParameter, certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                _logger.LogError(authResult.ErrorMessage);
                return new ErrorDetails
                {
                    Detail = authResult.ErrorMessage!,
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidClient
                };
            }

            // 2. Retrieve the granted token & check if it exists

            var grantedToken = await _tokenStore.GetAccessToken(refreshToken, cancellationToken)
                .ConfigureAwait(false);
            var isAccessToken = true;
            if (grantedToken == null)
            {
                grantedToken = await _tokenStore.GetRefreshToken(refreshToken, cancellationToken)
                    .ConfigureAwait(false);
                isAccessToken = false;
            }

            if (grantedToken == null)
            {
                _logger.LogError(Strings.TheRefreshTokenIsNotValid);
                return new ErrorDetails
                {
                    Detail = Strings.TheTokenDoesntExist,
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidToken
                };
            }

            // 3. Verifies whether the token was issued to the client making the revocation request
            if (grantedToken.ClientId != client.ClientId)
            {
                _logger.LogError(Strings.TheRefreshTokenIsNotValid);
                return new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidToken,
                    Detail = string.Format(Strings.TheTokenHasNotBeenIssuedForTheGivenClientId, client.ClientId)
                };
            }

            var success = isAccessToken switch
            {
                // 4. Invalid the granted token
                true => await _tokenStore.RemoveAccessToken(grantedToken.AccessToken, cancellationToken)
                    .ConfigureAwait(false),
                false when grantedToken.RefreshToken != null => await _tokenStore
                    .RemoveRefreshToken(grantedToken.RefreshToken, cancellationToken)
                    .ConfigureAwait(false),
                _ => false
            };

            return success
                ? new Option.Success()
                : new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.RevokeFailed,
                    Detail = Strings.CouldNotRevokeToken
                };
        }
    }
}
