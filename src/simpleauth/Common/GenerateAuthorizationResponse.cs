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

namespace SimpleAuth.Common;

using Extensions;
using JwtToken;
using Parameters;
using Results;
using Shared;
using Shared.Models;
using SimpleAuth.Shared.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Api.Authorization;
using SimpleAuth.Events;
using SimpleAuth.Properties;
using SimpleAuth.Shared.Events.OAuth;

internal sealed class GenerateAuthorizationResponse
{
    private readonly IAuthorizationCodeStore _authorizationCodeStore;
    private readonly ITokenStore _tokenStore;
    private readonly JwtGenerator _jwtGenerator;
    private readonly IClientStore _clientStore;
    private readonly IConsentRepository _consentRepository;
    private readonly IJwksStore _jwksStore;
    private readonly IEventPublisher _eventPublisher;

    public GenerateAuthorizationResponse(
        IAuthorizationCodeStore authorizationCodeStore,
        ITokenStore tokenStore,
        IScopeRepository scopeRepository,
        IClientStore clientStore,
        IConsentRepository consentRepository,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _authorizationCodeStore = authorizationCodeStore;
        _tokenStore = tokenStore;
        _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore, logger);
        _eventPublisher = eventPublisher;
        _clientStore = clientStore;
        _consentRepository = consentRepository;
        _jwksStore = jwksStore;
    }

    public async Task<EndpointResult> Generate(
        EndpointResult endpointResult,
        AuthorizationParameter authorizationParameter,
        ClaimsPrincipal claimsPrincipal,
        Client client,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        var allowedTokenScopes = string.Empty;
        GrantedToken? grantedToken = null;
        var responses = authorizationParameter.ResponseType.ParseResponseTypes();
        var generateIdToken = await GenerateIdTokenPayload(
                claimsPrincipal,
                authorizationParameter,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (generateIdToken is Option<JwtPayload>.Result p)
        {
            var idTokenPayload = p.Item;
            var payload = await GenerateUserInformationPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    cancellationToken)
                .ConfigureAwait(false);
            if (payload is Option<JwtPayload>.Error er)
            {
                return EndpointResult.CreateBadRequestResult(er.Details);
            }

            var userInformationPayload = (payload as Option<JwtPayload>.Result)!.Item;
            if (responses.Contains(ResponseTypeNames.Token))
            {
                // 1. Generate an access token.

                var tokenScopes = authorizationParameter.Scope.ParseScopes();
                allowedTokenScopes = string.Join(' ', tokenScopes);
                grantedToken = await _tokenStore.GetValidGrantedToken(
                                       _jwksStore,
                                       allowedTokenScopes,
                                       client.ClientId,
                                       cancellationToken,
                                       idTokenJwsPayload: userInformationPayload,
                                       userInfoJwsPayload: idTokenPayload)
                                   .ConfigureAwait(false)
                               ?? await client.GenerateToken(
                                       _jwksStore,
                                       tokenScopes,
                                       issuerName,
                                       userInformationPayload,
                                       idTokenPayload,
                                       cancellationToken: cancellationToken,
                                       claimsPrincipal.Claims.Where(
                                               c => client.UserClaimsToIncludeInAuthToken.Any(
                                                   r => r.IsMatch(c.Type)))
                                           .ToArray())
                                   .ConfigureAwait(false);

                endpointResult = endpointResult with
                {
                    RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                        StandardAuthorizationResponseNames.AccessTokenName,
                        grantedToken.AccessToken)
                };
            }

            AuthorizationCode? authorizationCode = null;
            var authorizationParameterClientId = authorizationParameter.ClientId;
            if (responses.Contains(ResponseTypeNames.Code)) // 2. Generate an authorization code.
            {
                var subject = claimsPrincipal.GetSubject()!;
                var assignedConsent = await _consentRepository
                    .GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                    .ConfigureAwait(false);
                if (assignedConsent != null)
                {
                    if (authorizationParameterClientId == null
                        || authorizationParameter.RedirectUrl == null
                        || authorizationParameter.Scope == null)
                    {
                        throw new ArgumentException(Strings.MissingValues, nameof(authorizationParameter));
                    }

                    // Insert a temporary authorization code
                    // It will be used later to retrieve tha id_token or an access token.
                    authorizationCode = new AuthorizationCode
                    {
                        Code = Id.Create(),
                        RedirectUri = authorizationParameter.RedirectUrl,
                        CreateDateTime = DateTimeOffset.UtcNow,
                        ClientId = authorizationParameterClientId,
                        Scopes = authorizationParameter.Scope,
                        IdTokenPayload = idTokenPayload,
                        UserInfoPayLoad = userInformationPayload
                    };

                    endpointResult = endpointResult with
                    {
                        RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                            StandardAuthorizationResponseNames.AuthorizationCodeName,
                            authorizationCode.Code)
                    };
                }
            }

            _jwtGenerator.FillInOtherClaimsIdentityTokenPayload(
                idTokenPayload,
                authorizationCode == null ? string.Empty : authorizationCode.Code,
                grantedToken == null ? string.Empty : grantedToken.AccessToken,
                client);

            if (grantedToken != null)
                // 3. Insert the stateful access token into the DB OR insert the access token into the caching.
            {
                if (authorizationParameterClientId == null || authorizationParameter.ResponseType == null)
                {
                    throw new ArgumentException(Strings.MissingValues, nameof(authorizationParameter));
                }

                await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new TokenGranted(
                            Id.Create(),
                            claimsPrincipal.GetSubject(),
                            authorizationParameterClientId,
                            allowedTokenScopes,
                            authorizationParameter.ResponseType,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
            }

            if (authorizationCode != null) // 4. Insert the authorization code into the caching.
            {
                if (client.RequirePkce)
                {
                    authorizationCode = authorizationCode with
                    {
                        CodeChallenge = authorizationParameter.CodeChallenge ?? string.Empty,
                        CodeChallengeMethod = authorizationParameter.CodeChallengeMethod ?? string.Empty
                    };
                }

                await _authorizationCodeStore.Add(authorizationCode, cancellationToken).ConfigureAwait(false);
                await _eventPublisher.Publish(
                        new AuthorizationGranted(
                            Id.Create(),
                            claimsPrincipal.GetSubject(),
                            authorizationParameterClientId!,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
            }

            if (responses.Contains(ResponseTypeNames.IdToken))
            {
                var idToken = await _clientStore.GenerateIdToken(
                        authorizationParameterClientId!,
                        idTokenPayload,
                        _jwksStore,
                        cancellationToken)
                    .ConfigureAwait(false);
                endpointResult = endpointResult with
                {
                    RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                        StandardAuthorizationResponseNames.IdTokenName,
                        idToken)
                };
            }

            if (!string.IsNullOrWhiteSpace(authorizationParameter.State))
            {
                endpointResult = endpointResult with
                {
                    RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                        StandardAuthorizationResponseNames.StateName,
                        authorizationParameter.State)
                };
            }

            var sessionState = GetSessionState(
                authorizationParameterClientId,
                authorizationParameter.OriginUrl,
                authorizationParameter.SessionId);
            if (sessionState != null)
            {
                endpointResult = endpointResult with
                {
                    RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                        StandardAuthorizationResponseNames.SessionState,
                        sessionState)
                };
            }

            if (authorizationParameter.ResponseMode == ResponseModes.FormPost)
            {
                endpointResult = endpointResult with
                {
                    Type = ActionResultType.RedirectToAction,
                    RedirectInstruction = endpointResult.RedirectInstruction!.AddParameter(
                            "redirect_uri",
                            authorizationParameter.RedirectUrl?.AbsoluteUri) with
                        {
                            Action = SimpleAuthEndPoints.FormIndex
                        }
                };
            }

            // Set the response mode
            if (endpointResult.Type == ActionResultType.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseModes.None)
                {
                    var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                    var authorizationFlow = responseTypes.GetAuthorizationFlow(authorizationParameter.State);
                    switch (authorizationFlow)
                    {
                        case Option<AuthorizationFlow>.Error error:
                            return EndpointResult.CreateBadRequestResult(error.Details);
                        case Option<AuthorizationFlow>.Result r:
                            responseMode = CoreConstants.MappingAuthorizationFlowAndResponseModes[r.Item];
                            break;
                    }
                }

                endpointResult = endpointResult with
                {
                    RedirectInstruction = endpointResult.RedirectInstruction! with { ResponseMode = responseMode }
                };
            }

            return endpointResult;
        }
        var e = generateIdToken as Option<JwtPayload>.Error;
        return EndpointResult.CreateBadRequestResult(e!.Details);
    }

    private static string? GetSessionState(string? clientId, string? originUrl, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(originUrl)
            || string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var salt = Id.Create();
        var s = $"{clientId}{originUrl}{sessionId}";
        var hex = s.ToSha256Hash(salt);

        return string.Concat(hex.Base64Encode(), "==.", salt);
    }

    private async Task<Option<JwtPayload>> GenerateIdTokenPayload(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        return authorizationParameter.Claims != null
               && authorizationParameter.Claims.IsAnyIdentityTokenClaimParameter()
            ? await _jwtGenerator.GenerateFilteredIdTokenPayload(
                    claimsPrincipal,
                    authorizationParameter,
                    authorizationParameter.Claims.IdToken,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false)
            : await _jwtGenerator.GenerateIdTokenPayloadForScopes(
                    claimsPrincipal,
                    authorizationParameter,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>
    /// Generate the JWS payload for user information endpoint.
    /// If at least one claim is defined then returns the filtered resultKind
    /// Otherwise returns the default payload based on the scopes.
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <param name="authorizationParameter"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    private async Task<Option<JwtPayload>> GenerateUserInformationPayload(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter,
        CancellationToken cancellationToken)
    {
        if (authorizationParameter.Claims != null && authorizationParameter.Claims.IsAnyUserInfoClaimParameter())
        {
            return _jwtGenerator.GenerateFilteredUserInfoPayload(
                authorizationParameter.Claims.UserInfo,
                claimsPrincipal,
                authorizationParameter);
        }

        var payload = await _jwtGenerator.GenerateUserInfoPayloadForScope(
                claimsPrincipal,
                authorizationParameter,
                cancellationToken)
            .ConfigureAwait(false);
        return new Option<JwtPayload>.Result(payload);
    }
}