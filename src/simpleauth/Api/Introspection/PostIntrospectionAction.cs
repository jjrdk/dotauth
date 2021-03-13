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

namespace SimpleAuth.Api.Introspection
{
    using Parameters;
    using Shared;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;

    internal class PostIntrospectionAction
    {
        private readonly ITokenStore _tokenStore;

        public PostIntrospectionAction(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        public async Task<GenericResponse<OauthIntrospectionResponse>> Execute(
            IntrospectionParameter introspectionParameter,
            CancellationToken cancellationToken)
        {
            // Read this RFC for more information - https://www.rfc-editor.org/rfc/rfc7662.txt

            // 3. Retrieve the token type hint
            var tokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken;
            if (CoreConstants.AllStandardTokenTypeHintNames.Contains(introspectionParameter.TokenTypeHint))
            {
                tokenTypeHint = introspectionParameter.TokenTypeHint;
            }

            // 4. Trying to fetch the information about the access_token  || refresh_token
            var introspectionParameterToken = introspectionParameter.Token;
            var grantedToken = tokenTypeHint switch
            {
                _ when introspectionParameterToken == null => null,
                CoreConstants.StandardTokenTypeHintNames.AccessToken => await _tokenStore
                    .GetAccessToken(introspectionParameterToken, cancellationToken)
                    .ConfigureAwait(false),
                CoreConstants.StandardTokenTypeHintNames.RefreshToken => await _tokenStore
                    .GetRefreshToken(introspectionParameterToken, cancellationToken)
                    .ConfigureAwait(false),
                _ => null
            };

            // 5. Return an error if there's no granted token
            if (grantedToken == null)
            {
                return new GenericResponse<OauthIntrospectionResponse>
                {
                    Content = new OauthIntrospectionResponse(),
                    StatusCode = HttpStatusCode.OK
                };
            }

            // 6. Fill-in parameters
            //// default : Specify the other parameters : NBF & JTI
            var result = new OauthIntrospectionResponse
            {
                Scope = grantedToken.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType
            };

            if (grantedToken.UserInfoPayLoad != null)
            {
                var subject =
                    grantedToken.IdTokenPayLoad?.GetClaimValue(OpenIdClaimTypes.Subject);
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    result = result with { Subject = subject };
                }
            }

            // 7. Fill-in the other parameters
            if (grantedToken.IdTokenPayLoad != null)
            {
                var audiencesArr = grantedToken.IdTokenPayLoad.GetArrayValue(StandardClaimNames.Audiences);
                var subject =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Subject);
                var userName =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Name);

                result = result with
                {
                    Audience = string.Join(" ", audiencesArr),
                    IssuedAt = grantedToken.IdTokenPayLoad.Iat ?? 0,
                    Issuer = grantedToken.IdTokenPayLoad.Iss
                };
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    result = result with { Subject = subject };
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    result = result with { UserName = userName };
                }
            }

            // 8. Based on the expiration date disable OR enable the introspection resultKind
            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            result = result with { Active = DateTimeOffset.UtcNow < expirationDateTime };

            return new GenericResponse<OauthIntrospectionResponse>
            {
                Content = result,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
