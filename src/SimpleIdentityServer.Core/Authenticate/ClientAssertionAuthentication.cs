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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.JwtToken;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Authenticate
{
    public class ClientAssertionAuthentication : IClientAssertionAuthentication
    {
        private readonly IJwsParser _jwsParser;
        private readonly IClientStore _clientRepository;
        private readonly IJwtParser _jwtParser;

        public ClientAssertionAuthentication(
            IJwsParser jwsParser,
            IClientStore clientRepository,
            IJwtParser jwtParser)
        {
            _jwsParser = jwsParser;
            _clientRepository = clientRepository;
            _jwtParser = jwtParser;
        }

        /// <summary>
        /// Try to get the client id.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction.ClientAssertionType != ClientAssertionTypes.JwtBearer || string.IsNullOrWhiteSpace(instruction.ClientAssertion))
            {
                return string.Empty;
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJweToken = _jwtParser.IsJweToken(clientAssertion);
            var isJwsToken = _jwtParser.IsJwsToken(clientAssertion);
            if (isJweToken && isJwsToken)
            {
                return string.Empty;
            }

            // It's a JWE token then return the client_id from the HTTP body
            if (isJweToken)
            {
                return instruction.ClientIdFromHttpRequestBody;
            }

            // It's a JWS token then return the client_id from the token.
            var payload = _jwsParser.GetPayload(clientAssertion);
            if (payload == null)
            {
                return string.Empty;
            }

            return payload.Issuer;
        }

        public async Task<AuthenticationResult> AuthenticateClientWithPrivateKeyJwtAsync(AuthenticateInstruction instruction, string expectedIssuer)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJwsToken = _jwtParser.IsJwsToken(clientAssertion);
            if (!isJwsToken)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            }

            var jws = instruction.ClientAssertion;
            var jwsPayload = _jwsParser.GetPayload(jws);
            if (jwsPayload == null)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
            }

            var clientId = jwsPayload.Issuer;
            var payload = await _jwtParser.UnSignAsync(jws, clientId).ConfigureAwait(false);
            if (payload == null)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheSignatureIsNotCorrect);
            }

            return await ValidateJwsPayLoad(payload, expectedIssuer).ConfigureAwait(false);
        }

        public async Task<AuthenticationResult> AuthenticateClientWithClientSecretJwtAsync(AuthenticateInstruction instruction, string clientSecret, string expectedIssuer)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJweToken = _jwtParser.IsJweToken(clientAssertion);
            if (!isJweToken)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJweToken);
            }

            var jwe = instruction.ClientAssertion;
            var clientId = instruction.ClientIdFromHttpRequestBody;
            var jws = await _jwtParser.DecryptWithPasswordAsync(jwe, clientId, clientSecret).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(jws))
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheJweTokenCannotBeDecrypted);
            }

            var isJwsToken = _jwtParser.IsJwsToken(jws);
            if (!isJwsToken)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            }

            var jwsPayload = await _jwtParser.UnSignAsync(jws, clientId).ConfigureAwait(false);
            if (jwsPayload == null)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
            }

            return await ValidateJwsPayLoad(jwsPayload, expectedIssuer).ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> ValidateJwsPayLoad(JwsPayload jwsPayload, string expectedIssuer)
        {
            // The checks are coming from this url : http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
            var jwsIssuer = jwsPayload.Issuer;
            var jwsSubject = jwsPayload.GetStringClaim(Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject);
            var jwsAudiences = jwsPayload.Audiences;
            var expirationDateTime = jwsPayload.ExpirationTime.ConvertFromUnixTimestamp();
            Common.Models.Client client = null;
            // 1. Check the issuer is correct.
            if (!string.IsNullOrWhiteSpace(jwsIssuer))
            {
                client = await _clientRepository.GetById(jwsIssuer).ConfigureAwait(false);
            }

            // 2. Check the client is correct.
            if (client == null || jwsSubject != jwsIssuer)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientIdPassedInJwtIsNotCorrect);
            }

            // 3. Check if the audience is correct
            if (jwsAudiences == null ||
                !jwsAudiences.Any() ||
                !jwsAudiences.Any(j => j.Contains(expectedIssuer)))
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheAudiencePassedInJwtIsNotCorrect);
            }

            // 4. Check the expiration time
            if (DateTime.UtcNow > expirationDateTime)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheReceivedJwtHasExpired);
            }

            return new AuthenticationResult(client, null);
        }
    }
}
