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

namespace SimpleAuth
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using Shared.Models;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Repositories;

    internal static class ClientExtensions
    {
        public static async Task<TokenValidationParameters> CreateValidationParameters(this Client client, IJwksStore jwksStore, string audience = null, string issuer = null, CancellationToken cancellationToken = default)
        {
            var signingKeys = await client.GetSigningCredentials(jwksStore, cancellationToken).ConfigureAwait(false);
            var encryptionKeys = client.JsonWebKeys.GetEncryptionKeys().ToArray();
            if (encryptionKeys.Length == 0 && client.IdTokenEncryptedResponseAlg != null)
            {
                var key = await jwksStore.GetEncryptionKey(client.IdTokenEncryptedResponseAlg, cancellationToken).ConfigureAwait(false);

                encryptionKeys = new[] { key };
            }
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKeys = signingKeys.Select(x => x.Key).ToList(),
                TokenDecryptionKeys = encryptionKeys
            };
            if (audience != null)
            {
                parameters.ValidAudience = audience;
            }
            else
            {
                parameters.ValidateAudience = false;
            }
            if (issuer != null)
            {
                parameters.ValidIssuer = issuer;
            }
            else
            {
                parameters.ValidateIssuer = false;
            }

            return parameters;
        }
    }
}