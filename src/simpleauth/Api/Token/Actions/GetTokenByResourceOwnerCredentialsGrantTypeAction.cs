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

namespace SimpleAuth.Api.Token.Actions
{
    using Authenticate;
    using Errors;
    using Exceptions;
    using Helpers;
    using JwtToken;
    using Logging;
    using Parameters;
    using Shared;
    using Shared.Models;
    using System;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Validators;

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        private readonly ScopeValidator _scopeValidator;
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;
        private readonly AuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGrantedTokenHelper _grantedTokenHelper;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper,
            IClientStore clientStore,
            IJwtGenerator jwtGenerator,
            IClientHelper clientHelper,
            ITokenStore tokenStore,
            IEventPublisher eventPublisher,
            IGrantedTokenHelper grantedTokenHelper)
        {
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _scopeValidator = new ScopeValidator();
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
            _authenticateClient = new AuthenticateClient(clientStore);
            _jwtGenerator = jwtGenerator;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
            _eventPublisher = eventPublisher;
            _grantedTokenHelper = grantedTokenHelper;
        }

        public async Task<GrantedToken> Execute(ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue, X509Certificate2 certificate, string issuerName)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            // 1. Try to authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(resourceOwnerGrantTypeParameter, certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            // 2. Check the client.
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.password))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.password));
            }

            if (client.ResponseTypes == null || !client.ResponseTypes.Contains(ResponseTypeNames.Token) || !client.ResponseTypes.Contains(ResponseTypeNames.IdToken))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, string.Format(ErrorDescriptions.TheClientDoesntSupportTheResponseType, client.ClientId, "token id_token"));
            }

            // 3. Try to authenticate a resource owner
            var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(
                resourceOwnerGrantTypeParameter.UserName,
                resourceOwnerGrantTypeParameter.Password,
                resourceOwnerGrantTypeParameter.AmrValues).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant, ErrorDescriptions.ResourceOwnerCredentialsAreNotValid);
            }

            // 4. Check if the requested scopes are valid
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                var scopeValidation = _scopeValidator.Check(resourceOwnerGrantTypeParameter.Scope, client);
                if (!scopeValidation.IsValid)
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope, scopeValidation.ErrorMessage);
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 5. Generate the user information payload and store it.
            var claims = resourceOwner.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = resourceOwnerGrantTypeParameter.Scope
            };
            var payload = await _jwtGenerator.GenerateUserInfoPayloadForScopeAsync(claimsPrincipal, authorizationParameter).ConfigureAwait(false);
            var generatedToken = await _grantedTokenHelper.GetValidGrantedTokenAsync(allowedTokenScopes, client.ClientId, payload, payload).ConfigureAwait(false);
            if (generatedToken == null)
            {
                generatedToken = await _grantedTokenGeneratorHelper.GenerateToken(client, allowedTokenScopes, issuerName, null, payload, payload).ConfigureAwait(false);
                if (generatedToken.IdTokenPayLoad != null)
                {
                    _jwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad, client);
                    generatedToken.IdToken = await _clientHelper.GenerateIdTokenAsync(client, generatedToken.IdTokenPayLoad).ConfigureAwait(false);
                }

                await _tokenStore.AddToken(generatedToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                    new AccessToClientGranted(
                    Id.Create(),
                    client.ClientId,
                    generatedToken.AccessToken,
                    allowedTokenScopes,
                    DateTime.UtcNow)).ConfigureAwait(false);
            }

            return generatedToken;
        }
    }
}