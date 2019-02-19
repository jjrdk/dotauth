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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal static class ClientHelper
    {
        public static async Task<string> GenerateIdToken(this IClientStore clientStore, string clientId, JwtPayload jwsPayload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
            if (client == null)
            {
                return null;
            }

            return await GenerateIdToken(client, jwsPayload).ConfigureAwait(false);
        }

        public static Task<string> GenerateIdToken(this Client client, JwtPayload jwsPayload)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.CreateEncodedJwt(
                jwsPayload.Iss,
                client.ClientName,
                new ClaimsIdentity(jwsPayload.Claims),
                DateTime.UtcNow,
                DateTime.UtcNow.Add(client.TokenLifetime),
                DateTime.UtcNow,
                client.JsonWebKeys.GetSigningCredentials(client.IdTokenSignedResponseAlg).First(),
                client.IdTokenEncryptedResponseAlg != null
                    ? new EncryptingCredentials(
                        client.JsonWebKeys.GetEncryptionKeys().First(),
                        client.IdTokenEncryptedResponseAlg,
                        client.IdTokenEncryptedResponseEnc)
                    : null);
            return Task.FromResult(jwt);
        }
    }
}
