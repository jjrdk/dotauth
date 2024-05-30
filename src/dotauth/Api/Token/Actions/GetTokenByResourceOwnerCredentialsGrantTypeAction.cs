﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Api.Token.Actions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Authenticate;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.JwtToken;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class GetTokenByResourceOwnerCredentialsGrantTypeAction
{
    private readonly AuthenticateClient _authenticateClient;
    private readonly JwtGenerator _jwtGenerator;
    private readonly ITokenStore _tokenStore;
    private readonly IJwksStore _jwksStore;
    private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;
    private readonly IEventPublisher _eventPublisher;

    public GetTokenByResourceOwnerCredentialsGrantTypeAction(
        IClientStore clientStore,
        IScopeStore scopeRepository,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
        _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore, logger);
        _tokenStore = tokenStore;
        _jwksStore = jwksStore;
        _resourceOwnerServices = resourceOwnerServices.ToArray();
        _eventPublisher = eventPublisher;
    }

    public async Task<Option<GrantedToken>> Execute(
        ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
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
        if (client == null)
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidClient,
                Detail = authResult.ErrorMessage!
            };
        }

        // 2. Check the client.
        if (!client.GrantTypes.Contains(GrantTypes.Password))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    client.ClientId,
                    GrantTypes.Password)
            };
        }

        if (!client.ResponseTypes.Contains(ResponseTypeNames.Token)
            || !client.ResponseTypes.Contains(ResponseTypeNames.IdToken))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidResponse,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheResponseType,
                    client.ClientId,
                    "token id_token")
            };
        }

        // 3. Try to authenticate a resource owner
        var resourceOwner = await _resourceOwnerServices.Authenticate(
                resourceOwnerGrantTypeParameter.UserName!,
                resourceOwnerGrantTypeParameter.Password!,
                cancellationToken,
                resourceOwnerGrantTypeParameter.AmrValues)
            .ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidCredentials,
                Detail = Strings.ResourceOwnerCredentialsAreNotValid
            };
        }

        // 4. Check if the requested scopes are valid
        var allowedTokenScopes = string.Empty;

        if (!string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
        {
            var scopeValidation = resourceOwnerGrantTypeParameter.Scope.Check(client);
            if (!scopeValidation.IsValid)
            {
                return new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidScope,
                    Detail = scopeValidation.ErrorMessage!
                };
            }

            allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
        }

        // 5. Generate the user information payload and store it.
        var claims = resourceOwner.Claims;
        var claimsIdentity = new ClaimsIdentity(claims, "DotAuth");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authorizationParameter = new AuthorizationParameter
        {
            Scope = resourceOwnerGrantTypeParameter.Scope,
            ClientId = client.ClientId
        };
        var userInfo = await _jwtGenerator
            .GenerateUserInfoPayloadForScope(claimsPrincipal, authorizationParameter, cancellationToken)
            .ConfigureAwait(false);
        var generatedIdTokenPayload = await _jwtGenerator.GenerateFilteredIdTokenPayload(
            claimsPrincipal,
            authorizationParameter,
            [],
            issuerName,
            cancellationToken).ConfigureAwait(false);
        if (generatedIdTokenPayload is Option<JwtPayload>.Error error)
        {
            return error.Details;
        }

        var idPayload = (generatedIdTokenPayload as Option<JwtPayload>.Result)!.Item;
        var generatedToken = await _tokenStore.GetValidGrantedToken(
                _jwksStore,
                allowedTokenScopes,
                client.ClientId,
                idTokenJwsPayload: idPayload,
                userInfoJwsPayload: userInfo,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (generatedToken == null)
        {
            generatedToken = await client.GenerateToken(
                    _jwksStore,
                    allowedTokenScopes.Split(' '),
                    issuerName,
                    userInfo,
                    userInfo,
                    cancellationToken,
                    claimsIdentity.Claims.Where(
                            c => client.UserClaimsToIncludeInAuthToken.Any(r => r.IsMatch(c.Type)))
                        .ToArray())
                .ConfigureAwait(false);
            if (generatedToken.IdTokenPayLoad != null)
            {
                generatedToken = generatedToken with
                {
                    IdTokenPayLoad = JwtGenerator.UpdatePayloadDate(generatedToken.IdTokenPayLoad, client.TokenLifetime),
                    IdToken = await client.GenerateIdToken(
                            generatedToken.IdTokenPayLoad,
                            _jwksStore,
                            cancellationToken)
                        .ConfigureAwait(false)
                };
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

        return generatedToken;
    }
}