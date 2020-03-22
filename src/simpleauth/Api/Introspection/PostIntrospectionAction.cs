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

namespace SimpleAuth.Api.Introspection
{
    using Authenticate;
    using Parameters;
    using Shared;
    using Shared.Models;
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Responses;

    internal class PostIntrospectionAction
    {
        private readonly AuthenticateClient _authenticateClient;
        private readonly ITokenStore _tokenStore;

        public PostIntrospectionAction(IClientStore clientStore, ITokenStore tokenStore, IJwksStore jwksStore)
        {
            _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
            _tokenStore = tokenStore;
        }

        public async Task<IntrospectionResponse> Execute(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            string issuerName,
            CancellationToken cancellationToken)
        {
            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(introspectionParameter.Token))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.IntrospectionRequestNames.Token));
            }

            // 2. Authenticate the client
            var instruction = CreateAuthenticateInstruction(introspectionParameter, authenticationHeaderValue);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            if (authResult.Client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 3. Retrieve the token type hint
            var tokenTypeHint = CoreConstants.StandardTokenTypeHintNames.AccessToken;
            if (CoreConstants.AllStandardTokenTypeHintNames.Contains(introspectionParameter.TokenTypeHint))
            {
                tokenTypeHint = introspectionParameter.TokenTypeHint;
            }

            // 4. Trying to fetch the information about the access_token  || refresh_token
            GrantedToken grantedToken;
            if (tokenTypeHint == CoreConstants.StandardTokenTypeHintNames.AccessToken)
            {
                grantedToken =
                    await _tokenStore.GetAccessToken(introspectionParameter.Token, cancellationToken)
                        .ConfigureAwait(false)
                    ?? await _tokenStore.GetRefreshToken(introspectionParameter.Token, cancellationToken)
                        .ConfigureAwait(false);
            }
            else
            {
                grantedToken =
                    await _tokenStore.GetRefreshToken(introspectionParameter.Token, cancellationToken)
                        .ConfigureAwait(false)
                    ?? await _tokenStore.GetAccessToken(introspectionParameter.Token, cancellationToken)
                        .ConfigureAwait(false);
            }

            // 5. Throw an exception if there's no granted token
            if (grantedToken == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidToken, ErrorDescriptions.TheTokenIsNotValid);
            }

            // 6. Fill-in parameters
            //// default : Specifiy the other parameters : NBF & JTI
            var result = new IntrospectionResponse
            {
                Scope = grantedToken.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType
            };

            // 7. Fill-in the other parameters
            if (grantedToken.IdTokenPayLoad != null)
            {
                var audiences = string.Empty;
                var audiencesArr = grantedToken.IdTokenPayLoad.GetArrayValue(StandardClaimNames.Audiences);
                var issuedAt = grantedToken.IdTokenPayLoad.Iat;
                var issuer = grantedToken.IdTokenPayLoad.Iss;
                var subject =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Subject);
                var userName =
                    grantedToken.IdTokenPayLoad.GetClaimValue(OpenIdClaimTypes.Name);
                if (audiencesArr.Any())
                {
                    audiences = string.Join(" ", audiencesArr);
                }

                result.Audience = audiences;
                result.IssuedAt = issuedAt ?? 0;
                result.Issuer = issuer;
                result.Subject = subject;
                result.UserName = userName;
            }

            // 8. Based on the expiration date disable OR enable the introspection resultKind
            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTimeOffset.UtcNow > expirationDateTime;
            result.Active = !tokenIsExpired;

            return result;
        }

        private AuthenticateInstruction CreateAuthenticateInstruction(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = new AuthenticateInstruction
            {
                ClientAssertion = introspectionParameter.ClientAssertion,
                ClientAssertionType = introspectionParameter.ClientAssertionType,
                ClientIdFromHttpRequestBody = introspectionParameter.ClientId,
                ClientSecretFromHttpRequestBody = introspectionParameter.ClientSecret
            };

            if (authenticationHeaderValue != null && !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter))
            {
                var parameters = GetParameters(authenticationHeaderValue.Parameter);
                if (parameters != null && parameters.Length == 2)
                {
                    result.ClientIdFromAuthorizationHeader = parameters[0];
                    result.ClientSecretFromAuthorizationHeader = parameters[1];
                }
            }

            return result;
        }

        private static string[] GetParameters(string authorizationHeaderValue)
        {
            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }
    }
}
