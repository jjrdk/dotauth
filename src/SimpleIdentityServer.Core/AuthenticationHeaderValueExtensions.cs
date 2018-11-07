// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Core
{
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using Authenticate;
    using Parameters;
    using Shared;

    public static class AuthenticationHeaderValueExtensions
    {
        public static AuthenticateInstruction GetAuthenticateInstruction(this AuthenticationHeaderValue authenticationHeaderValue, GrantTypeParameter grantTypeParameter, X509Certificate2 certificate = null)
        {
            var result = grantTypeParameter == null
                ? new AuthenticateInstruction {Certificate = certificate}
                : new AuthenticateInstruction
                {
                    ClientAssertion = grantTypeParameter.ClientAssertion,
                    ClientAssertionType = grantTypeParameter.ClientAssertionType,
                    ClientIdFromHttpRequestBody = grantTypeParameter.ClientId,
                    ClientSecretFromHttpRequestBody = grantTypeParameter.ClientSecret,
                    Certificate = certificate
                };
            if (authenticationHeaderValue == null)
            {
                return result;
            }
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
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return new string[0];
            }

            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }
    }
}
