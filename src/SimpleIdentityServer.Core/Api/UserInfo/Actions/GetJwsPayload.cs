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

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Buffers;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.UserInfo.Actions
{
    using Json;
    using Jwt;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Repositories;

    public class GetJwsPayload : IGetJwsPayload
    {
        private readonly IGrantedTokenValidator _grantedTokenValidator;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientStore _clientRepository;
        private readonly ITokenStore _tokenStore;

        public GetJwsPayload(
            IGrantedTokenValidator grantedTokenValidator,
            IJwtGenerator jwtGenerator,
            IClientStore clientRepository,
            ITokenStore tokenStore)
        {
            _grantedTokenValidator = grantedTokenValidator;
            _jwtGenerator = jwtGenerator;
            _clientRepository = clientRepository;
            _tokenStore = tokenStore;
        }

        public async Task<IActionResult> Execute(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            // Check if the access token is still valid otherwise raise an authorization exception.
            GrantedTokenValidationResult valResult;
            if (!((valResult = await _grantedTokenValidator.CheckAccessTokenAsync(accessToken).ConfigureAwait(false)).IsValid))
            {
                throw new AuthorizationException(valResult.MessageErrorCode, valResult.MessageErrorDescription);
            }

            var grantedToken = await _tokenStore.GetAccessToken(accessToken).ConfigureAwait(false);
            var client = await _clientRepository.GetById(grantedToken.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidToken, string.Format(ErrorDescriptions.TheClientIdDoesntExist, grantedToken.ClientId));
            }

            var signedResponseAlg = client.GetUserInfoSignedResponseAlg();
            var userInformationPayload = grantedToken.UserInfoPayLoad;
            if (userInformationPayload == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidToken, ErrorDescriptions.TheTokenIsNotAValidResourceOwnerToken);
            }

            if (signedResponseAlg == null
                || signedResponseAlg.Value == JwsAlg.none)
            {
                var objectResult = new ObjectResult(grantedToken.UserInfoPayLoad)
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/json"));
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Converters.Add(new JwsPayloadConverter());
                objectResult.Formatters.Add(new JsonOutputFormatter(serializerSettings, ArrayPool<char>.Shared));
                return objectResult;
            }

            var jwt = await _jwtGenerator.SignAsync(userInformationPayload,
                signedResponseAlg.Value).ConfigureAwait(false);
            var encryptedResponseAlg = client.GetUserInfoEncryptedResponseAlg();
            var encryptedResponseEnc = client.GetUserInfoEncryptedResponseEnc();
            if (encryptedResponseAlg != null)
            {
                if (encryptedResponseEnc == null)
                {
                    encryptedResponseEnc = JweEnc.A128CBC_HS256;
                }

                jwt = await _jwtGenerator.EncryptAsync(jwt,
                    encryptedResponseAlg.Value,
                    encryptedResponseEnc.Value).ConfigureAwait(false);
            }

            return new ContentResult
            {
                Content = jwt,
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/jwt",
            };
        }
    }
}
