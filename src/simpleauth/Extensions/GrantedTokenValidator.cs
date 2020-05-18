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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal static class GrantedTokenValidator
    {
        public static async Task<GrantedTokenValidationResult> CheckGrantedToken(this GrantedToken grantedToken, IJwksStore jwksStore, CancellationToken cancellationToken = default)
        {
            if (grantedToken == null)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = Strings.TheTokenIsNotValid,
                    IsValid = false
                };
            }

            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTimeOffset.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                return new GrantedTokenValidationResult
                {
                    MessageErrorCode = ErrorCodes.InvalidToken,
                    MessageErrorDescription = Strings.TheTokenIsExpired,
                    IsValid = false
                };
            }

            var publicKeys = await jwksStore.GetPublicKeys(cancellationToken).ConfigureAwait(false);
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateActor = false,
                ValidAudience = grantedToken.ClientId,
                ValidateIssuer = false,
                IssuerSigningKeys = publicKeys.Keys
            };

            try
            {
                handler.ValidateToken(grantedToken.AccessToken, validationParameters, out _);

                return new GrantedTokenValidationResult { IsValid = true };
            }
            catch (Exception exception)
            {
                return new GrantedTokenValidationResult
                {
                    IsValid = false,
                    MessageErrorCode = exception.Message,
                    MessageErrorDescription = exception.Message
                };
            }
        }

        public static async Task<GrantedToken> GetValidGrantedToken(
            this ITokenStore tokenStore,
            IJwksStore jwksStore,
            string scopes,
            string clientId,
            CancellationToken cancellationToken = default,
            JwtPayload idTokenJwsPayload = null,
            JwtPayload userInfoJwsPayload = null)
        {
            var token = await tokenStore.GetToken(scopes, clientId, idTokenJwsPayload, userInfoJwsPayload, cancellationToken)
                .ConfigureAwait(false);
            if (token == null)
            {
                return null;
            }

            if ((await token.CheckGrantedToken(jwksStore, cancellationToken).ConfigureAwait(false)).IsValid)
            {
                return token;
            }

            await tokenStore.RemoveAccessToken(token.AccessToken, cancellationToken).ConfigureAwait(false);
            return null;
        }
    }
}
