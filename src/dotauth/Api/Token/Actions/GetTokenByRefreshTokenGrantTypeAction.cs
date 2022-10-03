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

namespace DotAuth.Api.Token.Actions;

using System;
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
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

internal sealed class GetTokenByRefreshTokenGrantTypeAction
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ITokenStore _tokenStore;
    private readonly IJwksStore _jwksRepository;
    private readonly IResourceOwnerRepository _resourceOwnerRepository;
    private readonly IClientStore _clientStore;
    private readonly AuthenticateClient _authenticateClient;

    public GetTokenByRefreshTokenGrantTypeAction(
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        IJwksStore jwksRepository,
        IResourceOwnerRepository resourceOwnerRepository,
        IClientStore clientStore)
    {
        _eventPublisher = eventPublisher;
        _tokenStore = tokenStore;
        _jwksRepository = jwksRepository;
        _resourceOwnerRepository = resourceOwnerRepository;
        _clientStore = clientStore;
        _authenticateClient = new AuthenticateClient(clientStore, jwksRepository);
    }

    public async Task<Option<GrantedToken>> Execute(
        RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        // 1. Try to authenticate the client
        var instruction = authenticationHeaderValue.GetAuthenticateInstruction(
            refreshTokenGrantTypeParameter,
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
        if (client.GrantTypes.All(x => x != GrantTypes.RefreshToken))
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    client.ClientId,
                    GrantTypes.RefreshToken)
            };
        }

        // 3. Validate parameters
        var grantedToken = await ValidateParameter(refreshTokenGrantTypeParameter, cancellationToken)
            .ConfigureAwait(false);
        if (grantedToken?.ClientId != client.ClientId)
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = Strings.TheRefreshTokenCanBeUsedOnlyByTheSameIssuer
            };
        }

        var sub = grantedToken.UserInfoPayLoad?.Sub;
        var additionalClaims = Array.Empty<Claim>();
        if (sub != null)
        {
            var resourceOwner = await _resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);
            additionalClaims = resourceOwner?.Claims
                                   .Where(c => client.UserClaimsToIncludeInAuthToken.Any(r => r.IsMatch(c.Type)))
                                   .ToArray()
                               ?? Array.Empty<Claim>();
        }

        // 4. Generate a new access token & insert it
        var option = await _clientStore.GenerateToken(
                _jwksRepository,
                grantedToken.ClientId,
                grantedToken.Scope.Split(' ', StringSplitOptions.TrimEntries),
                issuerName,
                cancellationToken,
                userInformationPayload: grantedToken.UserInfoPayLoad,
                idTokenPayload: grantedToken.IdTokenPayLoad,
                additionalClaims)
            .ConfigureAwait(false);
        if (option is Option<GrantedToken>.Error e)
        {
            return e;
        }
        var generatedToken = ((Option<GrantedToken>.Result)option).Item with
        {
            ParentTokenId = grantedToken.Id
        };
        // 5. Fill-in the id token
        if (generatedToken.IdTokenPayLoad != null)
        {
            generatedToken = generatedToken with
            {
                IdTokenPayLoad = JwtGenerator.UpdatePayloadDate(
                    generatedToken.IdTokenPayLoad,
                    authResult.Client?.TokenLifetime),
                IdToken = await _clientStore.GenerateIdToken(
                        generatedToken.ClientId,
                        generatedToken.IdTokenPayLoad,
                        _jwksRepository,
                        cancellationToken)
                    .ConfigureAwait(false)
            };
        }

        await _tokenStore.AddToken(generatedToken, cancellationToken).ConfigureAwait(false);
        await _eventPublisher.Publish(
                new TokenGranted(
                    Id.Create(),
                    generatedToken.UserInfoPayLoad?.Sub,
                    generatedToken.ClientId,
                    generatedToken.Scope,
                    GrantTypes.RefreshToken,
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        return generatedToken;
    }

    private async Task<GrantedToken?> ValidateParameter(
        RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
        CancellationToken cancellationToken)
    {
        var grantedToken = refreshTokenGrantTypeParameter.RefreshToken == null
            ? null
            : await _tokenStore.GetRefreshToken(refreshTokenGrantTypeParameter.RefreshToken, cancellationToken)
                .ConfigureAwait(false);

        return grantedToken;
    }
}