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
    using Shared.Models;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Validators;

    internal static class GrantedTokenHelper
    {
        public static async Task<GrantedToken> GetValidGrantedTokenAsync(
            this ITokenStore tokenStore,
            string scopes,
            string clientId,
            JwtPayload idTokenJwsPayload = null,
            JwtPayload userInfoJwsPayload = null)
        {
            if (string.IsNullOrWhiteSpace(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var token = await tokenStore.GetToken(scopes, clientId, idTokenJwsPayload, userInfoJwsPayload)
                .ConfigureAwait(false);
            if (token == null)
            {
                return null;
            }

            if (!token.CheckGrantedToken().IsValid)
            {
                await tokenStore.RemoveAccessToken(token.AccessToken).ConfigureAwait(false);
                return null;
            }

            return token;
        }
    }
}
