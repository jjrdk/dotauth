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
using System.Collections.Generic;

namespace SimpleAuth.Api.Token.Actions
{
    using Authenticate;
    using JwtToken;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.OAuth;

    internal class GetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly AuthenticateClient _authenticateClient;
        private readonly JwtGenerator _jwtGenerator;
        private readonly ITokenStore _tokenStore;
        private readonly IJwksStore _jwksStore;
        private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;
        private readonly IEventPublisher _eventPublisher;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IClientStore clientStore,
            IScopeRepository scopeRepository,
            ITokenStore tokenStore,
            IJwksStore jwksStore,
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
            IEventPublisher eventPublisher)
        {
            _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
            _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore);
            _tokenStore = tokenStore;
            _jwksStore = jwksStore;
            _resourceOwnerServices = resourceOwnerServices.ToArray();
            _eventPublisher = eventPublisher;
        }

        public async Task<GenericResponse<GrantedToken>> Execute(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            // 1. Try to authenticate the client
            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
                resourceOwnerGrantTypeParameter,
                certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (authResult.Client == null)
            {
                return new GenericResponse<GrantedToken>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidClient,
                        Detail = authResult.ErrorMessage
                    }
                };
            }

            // 2. Check the client.
            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantTypes.Password))
            {
                return new GenericResponse<GrantedToken>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidGrant,
                        Detail = string.Format(
                            ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                            client.ClientId,
                            GrantTypes.Password)
                    }
                };
            }

            if (client.ResponseTypes == null
                || !client.ResponseTypes.Contains(ResponseTypeNames.Token)
                || !client.ResponseTypes.Contains(ResponseTypeNames.IdToken))
            {
                return new GenericResponse<GrantedToken>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidResponse,
                        Detail = string.Format(
                            ErrorDescriptions.TheClientDoesntSupportTheResponseType,
                            client.ClientId,
                            "token id_token")
                    }
                };
            }

            // 3. Try to authenticate a resource owner
            var resourceOwner = await _resourceOwnerServices.Authenticate(
                    resourceOwnerGrantTypeParameter.UserName,
                    resourceOwnerGrantTypeParameter.Password,
                    cancellationToken,
                    resourceOwnerGrantTypeParameter.AmrValues)
                .ConfigureAwait(false);
            if (resourceOwner == null)
            {
                return new GenericResponse<GrantedToken>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Error = new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidCredentials,
                        Detail = ErrorDescriptions.ResourceOwnerCredentialsAreNotValid
                    }
                };
            }

            // 4. Check if the requested scopes are valid
            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
            {
                var scopeValidation = resourceOwnerGrantTypeParameter.Scope.Check(client);
                if (!scopeValidation.IsValid)
                {
                    return new GenericResponse<GrantedToken>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Error = new ErrorDetails
                        {
                            Status = HttpStatusCode.BadRequest,
                            Title = ErrorCodes.InvalidScope,
                            Detail = scopeValidation.ErrorMessage
                        }
                    };
                }

                allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
            }

            // 5. Generate the user information payload and store it.
            var claims = resourceOwner.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                Scope = resourceOwnerGrantTypeParameter.Scope,
                ClientId = client.ClientId
            };
            var userInfo = await _jwtGenerator
                .GenerateUserInfoPayloadForScope(claimsPrincipal, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);
            var idPayload = await _jwtGenerator.GenerateFilteredIdTokenPayload(
                claimsPrincipal,
                authorizationParameter,
                new List<ClaimParameter>(),
                issuerName,
                cancellationToken).ConfigureAwait(false);
            var generatedToken = await _tokenStore.GetValidGrantedToken(
                    _jwksStore,
                    allowedTokenScopes,
                    client.ClientId,
                    cancellationToken,
                    idTokenJwsPayload: idPayload,
                    userInfoJwsPayload: userInfo)
                .ConfigureAwait(false);
            if (generatedToken == null)
            {
                generatedToken = await client.GenerateToken(
                        _jwksStore,
                        allowedTokenScopes,
                        issuerName,
                        userInfo,
                        userInfo,
                        cancellationToken,
                        claimsIdentity.Claims.Where(
                                c => client.UserClaimsToIncludeInAuthToken?.Any(r => r.IsMatch(c.Type)) == true)
                            .ToArray())
                    .ConfigureAwait(false);
                if (generatedToken.IdTokenPayLoad != null)
                {
                    generatedToken.IdTokenPayLoad = JwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad, client?.TokenLifetime);
                    generatedToken.IdToken = await client.GenerateIdToken(
                            generatedToken.IdTokenPayLoad,
                            _jwksStore,
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                await _tokenStore.AddToken(generatedToken, cancellationToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new TokenGranted(
                            Id.Create(),
                            claimsIdentity.Name,
                            client.ClientId,
                            allowedTokenScopes,
                            GrantTypes.Password,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
            }

            return new GenericResponse<GrantedToken> { StatusCode = HttpStatusCode.OK, Content = generatedToken };
        }
    }
}
