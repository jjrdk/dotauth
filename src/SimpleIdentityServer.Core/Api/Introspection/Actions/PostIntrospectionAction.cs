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

namespace SimpleAuth.Api.Introspection.Actions
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Authenticate;
    using Errors;
    using Exceptions;
    using Logging;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using Validators;

    public class PostIntrospectionAction : IPostIntrospectionAction
    {
        private readonly IOAuthEventSource _oauthEventSource;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IIntrospectionParameterValidator _introspectionParameterValidator;
        private readonly ITokenStore _tokenStore;

        public PostIntrospectionAction(
            IOAuthEventSource oauthEventSource,
            IAuthenticateClient authenticateClient,
            IIntrospectionParameterValidator introspectionParameterValidator,
            ITokenStore tokenStore)
        {
            _oauthEventSource = oauthEventSource;
            _authenticateClient = authenticateClient;
            _introspectionParameterValidator = introspectionParameterValidator;
            _tokenStore = tokenStore;
        }

        public async Task<IntrospectionResult> Execute(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue, string issuerName)
        {            
            // 1. Validate the parameters
            if (introspectionParameter == null)
            {
                throw new ArgumentNullException(nameof(introspectionParameter));
            }

            if (string.IsNullOrWhiteSpace(introspectionParameter.Token))
            {
                throw new ArgumentNullException(nameof(introspectionParameter.Token));
            }

            _introspectionParameterValidator.Validate(introspectionParameter);

            // 2. Authenticate the client
            var instruction = CreateAuthenticateInstruction(introspectionParameter, authenticationHeaderValue);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName).ConfigureAwait(false);
            if (authResult.Client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
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
                grantedToken = await _tokenStore.GetAccessToken(introspectionParameter.Token).ConfigureAwait(false) ??
                               await _tokenStore.GetRefreshToken(introspectionParameter.Token).ConfigureAwait(false);
            }
            else
            {
                grantedToken = await _tokenStore.GetRefreshToken(introspectionParameter.Token).ConfigureAwait(false) ??
                               await _tokenStore.GetAccessToken(introspectionParameter.Token).ConfigureAwait(false);
            }

            // 5. Throw an exception if there's no granted token
            if (grantedToken == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidToken,
                    ErrorDescriptions.TheTokenIsNotValid);
            }

            // 6. Fill-in parameters
            //// TODO : Specifiy the other parameters : NBF & JTI
            var result = new IntrospectionResult
            {
                Scope = grantedToken.Scope,
                ClientId = grantedToken.ClientId,
                Expiration = grantedToken.ExpiresIn,
                TokenType = grantedToken.TokenType
            };

            // 7. Fill-in the other parameters
            if (grantedToken.IdTokenPayLoad != null)
            {
                var audiences = string.Empty;
                var audiencesArr = grantedToken.IdTokenPayLoad.GetArrayClaim(StandardClaimNames.Audiences);
                var issuedAt = grantedToken.IdTokenPayLoad.Iat;
                var issuer = grantedToken.IdTokenPayLoad.Issuer;
                var subject = grantedToken.IdTokenPayLoad.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Subject);
                var userName = grantedToken.IdTokenPayLoad.GetStringClaim(JwtConstants.StandardResourceOwnerClaimNames.Name);
                if (audiencesArr.Any())
                {
                    audiences = string.Join(" ", audiencesArr);
                }

                result.Audience = audiences;
                result.IssuedAt = issuedAt;
                result.Issuer = issuer;
                result.Subject = subject;
                result.UserName = userName;
            }

            // 8. Based on the expiration date disable OR enable the introspection result
            var expirationDateTime = grantedToken.CreateDateTime.AddSeconds(grantedToken.ExpiresIn);
            var tokenIsExpired = DateTime.UtcNow > expirationDateTime;
            if (tokenIsExpired)
            {
                result.Active = false;
            }
            else
            {
                result.Active = true;
            }

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

            if (authenticationHeaderValue != null
                && !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter))
            {
                var parameters = GetParameters(authenticationHeaderValue.Parameter);
                if (parameters != null && parameters.Count() == 2)
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
