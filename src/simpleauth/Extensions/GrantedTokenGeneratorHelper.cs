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

namespace SimpleAuth.Extensions
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal static class GrantedTokenGeneratorHelper
    {
        public static async Task<GrantedToken> GenerateToken(
            this IClientStore clientStore,
            string clientId,
            string scope,
            string issuerName,
            CancellationToken cancellationToken,
            JwtPayload userInformationPayload = null,
            JwtPayload idTokenPayload = null,
            params Claim[] additionalClaims)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, ErrorDescriptions.TheClientIdDoesntExist);
            }

            return await GenerateToken(
                    client,
                    scope,
                    issuerName,
                    userInformationPayload,
                    idTokenPayload,
                    additionalClaims)
                .ConfigureAwait(false);
        }

        public static Task<GrantedToken> GenerateToken(
            this Client client,
            string scope,
            string issuerName,
            JwtPayload userInformationPayload = null,
            JwtPayload idTokenPayload = null,
            params Claim[] additionalClaims)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var handler = new JwtSecurityTokenHandler();
            var enumerable =
                new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, client.ClientName ?? client.ClientId),
                        new Claim(StandardClaimNames.Scopes, scope),
                        new Claim("client_id", client.ClientId),
                    }.Concat(client.Claims ?? Array.Empty<Claim>())
                    .Concat(additionalClaims ?? Array.Empty<Claim>())
                    .GroupBy(x => x.Type)
                    .Select(x => x.First());

            if (idTokenPayload != null && idTokenPayload.Iss == null)
            {
                idTokenPayload.AddClaim(new Claim(StandardClaimNames.Issuer, issuerName));
            }

            var signingCredentials = client.JsonWebKeys.GetSigningCredentials(client.IdTokenSignedResponseAlg).First();

            var accessToken = handler.CreateEncodedJwt(
                issuerName,
                client.ClientId,
                new ClaimsIdentity(enumerable),
                DateTime.UtcNow,
                DateTime.UtcNow.Add(client.TokenLifetime),
                DateTime.UtcNow,
                signingCredentials);

            var refreshTokenId = Encoding.UTF8.GetBytes(Id.Create());
            // 3. Construct the refresh token.
            return Task.FromResult(new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = (int)client.TokenLifetime.TotalSeconds,
                TokenType = CoreConstants.StandardTokenTypes._bearer,
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
