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
    using Microsoft.IdentityModel.Tokens;
    using Shared.Repositories;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Logging;
    using SimpleAuth.Shared.Errors;

    internal class ClientAssertionAuthentication
    {
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        private readonly IClientStore _clientRepository;
        private readonly IJwksStore _jwksStore;

        public ClientAssertionAuthentication(IClientStore clientRepository, IJwksStore jwksStore)
        {
            IdentityModelEventSource.ShowPII = true;
            _clientRepository = clientRepository;
            _jwksStore = jwksStore;
        }

        /// <summary>
        /// Try to get the client id.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction.ClientAssertionType != "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
                || string.IsNullOrWhiteSpace(instruction.ClientAssertion))
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

        public async Task<AuthenticationResult> AuthenticateClientWithPrivateKeyJwt(
            AuthenticateInstruction instruction,
            string expectedIssuer,
            CancellationToken cancellationToken)
        {
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
            var client = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);

            try
            {
                var validationParameters = await client.CreateValidationParameters(_jwksStore, expectedIssuer, clientId).ConfigureAwait(false);
                _handler.ValidateToken(
                    instruction.ClientAssertion,
                    validationParameters,
                    out var securityToken);
                var payload = (securityToken as JwtSecurityToken)?.Payload;
                return payload == null
                    ? new AuthenticationResult(null, ErrorDescriptions.TheSignatureIsNotCorrect)
                    : new AuthenticationResult(client, null);
            }
            catch (SecurityTokenValidationException validationException)
            {
                return new AuthenticationResult(null, validationException.Message);
            }
        }

        public async Task<AuthenticationResult> AuthenticateClientWithClientSecretJwt(
            AuthenticateInstruction instruction,
            CancellationToken cancellationToken)
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
            var client = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
            var validationParameters = await client.CreateValidationParameters(_jwksStore).ConfigureAwait(false);
            _handler.ValidateToken(jwe, validationParameters, out var securityToken);
            var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;

            return jwsPayload == null
                ? new AuthenticationResult(null, ErrorDescriptions.TheJwsPayloadCannotBeExtracted)
                : new AuthenticationResult(client, null);
        }
    }
}
