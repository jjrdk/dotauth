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

namespace SimpleAuth.Authenticate
{
    using Errors;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Repositories;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;

    internal class ClientAssertionAuthentication
    {
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        private readonly IClientStore _clientRepository;

        public ClientAssertionAuthentication(
            IClientStore clientRepository)
        {
            _clientRepository = clientRepository;
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
            var isJweToken = clientAssertion.IsJweToken();
            var isJwsToken = clientAssertion.IsJwsToken();
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
            var token = new JwtSecurityToken(clientAssertion);

            return token.Issuer ?? string.Empty;
        }

        public async Task<AuthenticationResult> AuthenticateClientWithPrivateKeyJwtAsync(
            AuthenticateInstruction instruction,
            string expectedIssuer)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            var isJwsToken = instruction.ClientAssertion.IsJwsToken();
            if (!isJwsToken)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            }

            //var jws = instruction.ClientAssertion;
            var jwsPayload = new JwtSecurityToken(instruction.ClientAssertion); //_jwsParser.GetPayload(jws);
            if (jwsPayload.Payload == null)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
            }

            var clientId = jwsPayload.Issuer;
            var client = await _clientRepository.GetById(clientId).ConfigureAwait(false);

            try
            {
                _handler.ValidateToken(
                    instruction.ClientAssertion,
                    client.CreateValidationParameters(expectedIssuer, clientId),
                    out var securityToken);
                var payload = (securityToken as JwtSecurityToken)?.Payload;
                //await _jwtParser.UnSignAsync(jws, clientId).ConfigureAwait(false);
                return payload == null
                    ? new AuthenticationResult(null, ErrorDescriptions.TheSignatureIsNotCorrect)
                    : new AuthenticationResult(client, null);
            }
            catch (SecurityTokenValidationException validationException)
            {
                return new AuthenticationResult(null, validationException.Message);
            }
        }

        public async Task<AuthenticationResult> AuthenticateClientWithClientSecretJwtAsync(AuthenticateInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            var clientAssertion = instruction.ClientAssertion;
            var isJweToken = clientAssertion.IsJweToken();
            if (!isJweToken)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJweToken);
            }

            var jwe = instruction.ClientAssertion;
            var clientId = instruction.ClientIdFromHttpRequestBody;
            var client = await _clientRepository.GetById(clientId).ConfigureAwait(false);
            var validationParameters = client.CreateValidationParameters();
            _handler.ValidateToken(jwe, validationParameters, out var securityToken);
            var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;
            //var jws = await _jwtParser.DecryptWithPasswordAsync(jwe, clientId, clientSecret).ConfigureAwait(false);
            //if (string.IsNullOrWhiteSpace(jws))
            //{
            //    return new AuthenticationResult(null, ErrorDescriptions.TheJweTokenCannotBeDecrypted);
            //}

            //var isJwsToken = _jwtParser.IsJwsToken(jws);
            //if (!isJwsToken)
            //{
            //    return new AuthenticationResult(null, ErrorDescriptions.TheClientAssertionIsNotAJwsToken);
            //}

            //var jwsPayload = await _jwtParser.UnSignAsync(jws, clientId).ConfigureAwait(false);
            if (jwsPayload == null)
            {
                return new AuthenticationResult(null, ErrorDescriptions.TheJwsPayloadCannotBeExtracted);
            }

            return new AuthenticationResult(client, null);
        }
    }
}
