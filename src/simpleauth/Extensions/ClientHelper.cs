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
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;

    internal static class ClientHelper
    {
        public static async Task<string> GenerateIdToken(
            this IClientStore clientStore, string clientId, JwtPayload jwsPayload, IJwksStore jwksStore,
            CancellationToken cancellationToken)
        {
            var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
            return client == null
                ? null
                : await GenerateIdToken(client, jwsPayload, jwksStore, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<List<SigningCredentials>> GetSigningCredentials(this Client client, IJwksStore jwksStore, CancellationToken cancellationToken = default)
        {
            var signingKeys = client.JsonWebKeys?.Keys.Where(key => key.Use == JsonWebKeyUseNames.Sig)
                .Select(key => new SigningCredentials(key, key.Alg))
                .ToList();
            if (signingKeys?.Count == 0)
            {
                var keys = await (client.IdTokenSignedResponseAlg == null
                    ? jwksStore.GetDefaultSigningKey(cancellationToken)
                    : jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg, cancellationToken)).ConfigureAwait(false);

                signingKeys = new List<SigningCredentials> { keys };
            }

            return signingKeys;
        }

        public static async Task<string> GenerateIdToken(
            this Client client, JwtPayload jwsPayload, IJwksStore jwksStore, CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var signingCredentials = await client.GetSigningCredentials(jwksStore, cancellationToken).ConfigureAwait(false);
            var jwt = handler.CreateEncodedJwt(
                jwsPayload.Iss,
                client.ClientName,
                new ClaimsIdentity(jwsPayload.Claims),
                DateTime.UtcNow,
                DateTime.UtcNow.Add(client.TokenLifetime),
                DateTime.UtcNow,
                signingCredentials.First());

            return jwt;
        }
    }
}
