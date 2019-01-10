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

namespace SimpleAuth.Helpers
{
    using Errors;
    using Exceptions;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    public class GrantedTokenGeneratorHelper : IGrantedTokenGeneratorHelper
    {
        private readonly IClientStore _clientRepository;

        public GrantedTokenGeneratorHelper(IClientStore clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<GrantedToken> GenerateToken(
            string clientId,
            string scope,
            string issuerName,
            IDictionary<string, object> additionalClaims,
            JwtPayload userInformationPayload = null,
            JwtPayload idTokenPayload = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _clientRepository.GetById(clientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, ErrorDescriptions.TheClientIdDoesntExist);
            }

            return await GenerateToken(
                    client,
                    scope,
                    issuerName,
                    null,
                    userInformationPayload,
                    idTokenPayload)
                .ConfigureAwait(false);
        }

        public Task<GrantedToken> GenerateToken(
            Client client,
            string scope,
            string issuerName,
            IDictionary<string, object> additionalClaims,
            JwtPayload userInformationPayload = null,
            JwtPayload idTokenPayload = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            //var expiresIn =
            //    _configurationService.TokenValidityPeriod; // 1. Retrieve the expiration time of the granted token.
            //var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scope.Split(' '), issuerName, additionalClaims).ConfigureAwait(false); // 2. Construct the JWT token (client).
            var handler = new JwtSecurityTokenHandler();
            var enumerable = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.ClientId),
                new Claim(StandardClaimNames.Scopes, scope)
            };
            //.Concat(additionalClaims?.Select(x => new Claim(x.Key, x.Value.ToString())));

            var signingCredentials = client.JsonWebKeys.GetSigningCredentials(client.IdTokenSignedResponseAlg).First();

            var accessToken = handler.CreateEncodedJwt(
                issuerName,
                client.ClientId,
                new ClaimsIdentity(enumerable),
                DateTime.UtcNow,
                DateTime.UtcNow.Add(client.TokenLifetime),
                DateTime.UtcNow,
                signingCredentials);
            //var accessToken = await _clientHelper.GenerateIdTokenAsync(client, jwsPayload).ConfigureAwait(false);

            var refreshTokenId = Encoding.UTF8.GetBytes(Id.Create());
            // 3. Construct the refresh token.
            return Task.FromResult(new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = (int)client.TokenLifetime.TotalSeconds,
                TokenType = CoreConstants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                // IDS
                Scope = scope,
                UserInfoPayLoad = userInformationPayload,
                IdTokenPayLoad = idTokenPayload,
                ClientId = client.ClientId
            });
        }
    }
}
