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

namespace SimpleAuth.Api.Token;

using Actions;
using Authenticate;
using Parameters;
using Shared;
using Shared.Events.OAuth;
using Shared.Models;
using SimpleAuth.Extensions;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Events;
using SimpleAuth.Properties;
using SimpleAuth.Services;

internal sealed class TokenActions
{
    private readonly GetTokenByDeviceAuthorizationTypeAction _getTokenByDeviceAuthorizationTypeAction;
    private readonly GetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;
    private readonly GetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;
    private readonly GetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;
    private readonly AuthenticateClient _authenticateClient;
    private readonly RevokeTokenAction _revokeTokenAction;
    private readonly IJwksStore _jwksStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITokenStore _tokenStore;

    public TokenActions(
        RuntimeSettings simpleAuthOptions,
        IAuthorizationCodeStore authorizationCodeStore,
        IClientStore clientStore,
        IScopeStore scopeRepository,
        IJwksStore jwksStore,
        IResourceOwnerRepository resourceOwnerRepository,
        IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        IDeviceAuthorizationStore deviceAuthorizationStore,
        ILogger logger)
    {
        _getTokenByDeviceAuthorizationTypeAction = new GetTokenByDeviceAuthorizationTypeAction(
            deviceAuthorizationStore,
            tokenStore,
            jwksStore,
            clientStore,
            eventPublisher,
            logger);
        _getTokenByResourceOwnerCredentialsGrantType = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
            clientStore,
            scopeRepository,
            tokenStore,
            jwksStore,
            resourceOwnerServices,
            eventPublisher,
            logger);
        _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
            authorizationCodeStore,
            simpleAuthOptions,
            clientStore,
            eventPublisher,
            tokenStore,
            jwksStore);
        _getTokenByRefreshTokenGrantTypeAction = new GetTokenByRefreshTokenGrantTypeAction(
            eventPublisher,
            tokenStore,
            jwksStore,
            resourceOwnerRepository,
            clientStore);
        _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
        _revokeTokenAction = new RevokeTokenAction(clientStore, tokenStore, jwksStore, logger);
        _jwksStore = jwksStore;
        _eventPublisher = eventPublisher;
        _tokenStore = tokenStore;
    }

    public async Task<Option<GrantedToken>> GetTokenByResourceOwnerCredentialsGrantType(
        ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.UserName))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(Strings.MissingParameter, StandardTokenRequestParameterNames.UserName)
            };
        }

        if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Password))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(
                    Strings.MissingParameter,
                    StandardTokenRequestParameterNames.PasswordName)
            };
        }

        if (string.IsNullOrWhiteSpace(resourceOwnerGrantTypeParameter.Scope))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(Strings.MissingParameter, StandardTokenRequestParameterNames.ScopeName)
            };
        }

        return await _getTokenByResourceOwnerCredentialsGrantType.Execute(
            resourceOwnerGrantTypeParameter,
            authenticationHeaderValue,
            certificate,
            issuerName,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<Option<GrantedToken>> GetTokenByAuthorizationCodeGrantType(
        AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        var validation = Validate(authorizationCodeGrantTypeParameter);
        if (validation is Option.Error e)
        {
            return e.Details;
        }

        return await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
            authorizationCodeGrantTypeParameter,
            authenticationHeaderValue,
            certificate,
            issuerName,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<Option<GrantedToken>> GetTokenByRefreshTokenGrantType(
        RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        // Read this RFC for more information
        if (string.IsNullOrWhiteSpace(refreshTokenGrantTypeParameter.RefreshToken))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(
                    Strings.MissingParameter,
                    StandardTokenRequestParameterNames.RefreshToken)
            };
        }

        return await _getTokenByRefreshTokenGrantTypeAction.Execute(
            refreshTokenGrantTypeParameter,
            authenticationHeaderValue,
            certificate,
            issuerName,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<Option<GrantedToken>> GetTokenByClientCredentialsGrantType(
        ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(Strings.MissingParameter, StandardTokenRequestParameterNames.ScopeName)
            };
        }

        // 1. Authenticate the client
        var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
            clientCredentialsGrantTypeParameter,
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

        // 2. Check client
        if (client.GrantTypes.All(x => x != GrantTypes.ClientCredentials))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    client.ClientId,
                    GrantTypes.ClientCredentials)
            };
        }

        if (!client.ResponseTypes.Contains(ResponseTypeNames.Token))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidClient,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheResponseType,
                    client.ClientId,
                    ResponseTypeNames.Token)
            };
        }

        // 3. Check scopes
        var allowedTokenScopes = string.Empty;
        if (!string.IsNullOrWhiteSpace(clientCredentialsGrantTypeParameter.Scope))
        {
            var scopeValidation = clientCredentialsGrantTypeParameter.Scope.Check(client);
            if (!scopeValidation.IsValid)
            {
                return new ErrorDetails
                {
                    Status = HttpStatusCode.BadRequest,
                    Title = ErrorCodes.InvalidScope,
                    Detail = string.Format(
                        Strings.ScopesAreNotAllowedOrInvalid,
                        clientCredentialsGrantTypeParameter.Scope)
                };
            }

            allowedTokenScopes = string.Join(" ", scopeValidation.Scopes);
        }

        // 4. Generate the JWT access token on the fly.
        var grantedToken = await _tokenStore
            .GetValidGrantedToken(_jwksStore, allowedTokenScopes, client.ClientId, cancellationToken)
            .ConfigureAwait(false);
        if (grantedToken == null)
        {
            grantedToken = await client.GenerateToken(
                    _jwksStore,
                    allowedTokenScopes.Split(' '),
                    issuerName,
                    new JwtPayload(client.Claims),
                    additionalClaims: client.Claims.Where(
                            c => client.UserClaimsToIncludeInAuthToken.Any(r => r.IsMatch(c.Type)))
                        .ToArray(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);
            await _eventPublisher.Publish(
                    new TokenGranted(
                        Id.Create(),
                        grantedToken?.UserInfoPayLoad?.Sub,
                        grantedToken!.ClientId,
                        grantedToken.Scope,
                        GrantTypes.ClientCredentials,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
        }

        return grantedToken;
    }

    public async Task<Option<GrantedToken>> GetTokenByDeviceGrantType(string? clientId, string? deviceCode, string issuerName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(deviceCode))
        {
            return new ErrorDetails
            {
                Title = ErrorCodes.InvalidClient,
                Detail = Strings.ClientIsNotValid,
                Status = HttpStatusCode.BadRequest
            };
        }
        return await _getTokenByDeviceAuthorizationTypeAction.Execute(
            clientId,
            deviceCode,
            issuerName,
            cancellationToken).ConfigureAwait(false);

    }

    public async Task<Option> RevokeToken(
        RevokeTokenParameter revokeTokenParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        // Read this RFC for more information
        if (string.IsNullOrWhiteSpace(revokeTokenParameter.Token))
        {
            return new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(
                    Strings.MissingParameter,
                    CoreConstants.IntrospectionRequestNames.Token),
                Status = HttpStatusCode.BadRequest
            };
        }

        var result = await _revokeTokenAction.Execute(
                revokeTokenParameter,
                authenticationHeaderValue,
                certificate,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (result is Option.Error e)
        {
            return e.Details;
        }

        await _eventPublisher
            .Publish(new TokenRevoked(Id.Create(), revokeTokenParameter.Token, DateTimeOffset.UtcNow))
            .ConfigureAwait(false);

        return new Option.Success();
    }

    private static Option Validate(AuthorizationCodeGrantTypeParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter.Code))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(
                    Strings.MissingParameter,
                    StandardTokenRequestParameterNames.AuthorizationCodeName)
            };
        }

        // With this instruction
        // The redirect_uri is considered well-formed according to the RFC-3986
        var redirectUrlIsCorrect = parameter.RedirectUri?.IsAbsoluteUri;
        if (redirectUrlIsCorrect != true)
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = Strings.TheRedirectionUriIsNotWellFormed
            };
        }

        return new Option.Success();
    }
}