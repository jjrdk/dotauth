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
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;

    internal static class GrantedTokenGeneratorHelper
    {
        public static async Task<GrantedToken> GenerateToken(
            this IClientStore clientStore,
            IJwksStore jwksStore,
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
                throw new SimpleAuthException(ErrorCodes.InvalidClient, Strings.TheClientDoesntExist);
            }

            return await GenerateToken(
                    client,
                    jwksStore,
                    scope,
                    issuerName,
                    userInformationPayload,
                    idTokenPayload,
                    cancellationToken,
                    additionalClaims)
                .ConfigureAwait(false);
        }

        public static async Task<GrantedToken> GenerateToken(
            this Client client,
            IJwksStore jwksStore,
            string scope,
            string issuerName,
            JwtPayload userInformationPayload = null,
            JwtPayload idTokenPayload = null,
            CancellationToken cancellationToken = default,
            params Claim[] additionalClaims)
        {
            var handler = new JwtSecurityTokenHandler();
            var enumerable =
                new[]
                    {
                        new Claim(StandardClaimNames.Scopes, string.Join(' ', scope)),
                        new Claim(StandardClaimNames.Azp, client.ClientId),
                    }.Concat(client.Claims ?? Array.Empty<Claim>())
                    .Concat(additionalClaims ?? Array.Empty<Claim>())
                    .GroupBy(x => x.Type)
                    .Select(x => new Claim(x.Key, string.Join(" ", x.Select(y => y.Value))));

            if (idTokenPayload != null && idTokenPayload.Iss == null)
            {
                idTokenPayload.AddClaim(new Claim(StandardClaimNames.Issuer, issuerName));
            }

            var signingCredentials = await jwksStore.GetSigningKey(client.TokenEndPointAuthSigningAlg, cancellationToken).ConfigureAwait(false);

            //var tokenLifetime = scope.Contains("uma_protection", StringComparison.Ordinal) ? client.TokenLifetime
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
            return new GrantedToken
            {
                Id = Id.Create(),
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = (int)client.TokenLifetime.TotalSeconds,
                TokenType = CoreConstants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTimeOffset.UtcNow,
                // IDS
                Scope = scope,
                UserInfoPayLoad = userInformationPayload,
                IdTokenPayLoad = idTokenPayload,
                ClientId = client.ClientId
            };
        }
    }
}
