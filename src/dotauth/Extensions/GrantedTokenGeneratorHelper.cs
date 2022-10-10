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

namespace DotAuth.Extensions;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;

internal static class GrantedTokenGeneratorHelper
{
    public static async Task<Option<GrantedToken>> GenerateToken(
        this IClientStore clientStore,
        IJwksStore jwksStore,
        string clientId,
        string[] scope,
        string issuerName,
        CancellationToken cancellationToken,
        JwtPayload? userInformationPayload = null,
        JwtPayload? idTokenPayload = null,
        params Claim[] additionalClaims)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return new Option<GrantedToken>.Error(new ErrorDetails
            {
                Title = ErrorCodes.InvalidClient,
                Detail = SharedStrings.TheClientDoesntExist,
                Status = HttpStatusCode.NotFound
            });
        }

        var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return new Option<GrantedToken>.Error(new ErrorDetails
            {
                Title = ErrorCodes.InvalidClient,
                Detail = SharedStrings.TheClientDoesntExist,
                Status = HttpStatusCode.NotFound
            });
        }

        var token = await GenerateToken(
                client,
                jwksStore,
                scope,
                issuerName,
                userInformationPayload,
                idTokenPayload,
                cancellationToken,
                additionalClaims)
            .ConfigureAwait(false);
        return new Option<GrantedToken>.Result(token);
    }

    public static async Task<GrantedToken> GenerateToken(
        this Client client,
        IJwksStore jwksStore,
        string[] scopes,
        string issuerName,
        JwtPayload? userInformationPayload = null,
        JwtPayload? idTokenPayload = null,
        CancellationToken cancellationToken = default,
        params Claim[] additionalClaims)
    {
        var handler = new JwtSecurityTokenHandler();
        var scopeString = string.Join(' ', scopes);
        var enumerable =
            new[]
                {
                    new Claim(StandardClaimNames.Scopes, scopeString),
                    new Claim(StandardClaimNames.Azp, client.ClientId),
                }.Concat(client.Claims)
                .Concat(additionalClaims)
                .GroupBy(x => x.Type)
                .Select(x => new Claim(x.Key, string.Join(" ", x.Select(y => y.Value))));

        if (idTokenPayload is {Iss: null})
        {
            idTokenPayload.AddClaim(new Claim(StandardClaimNames.Issuer, issuerName));
        }

        var signingCredentials = await jwksStore.GetSigningKey(client.TokenEndPointAuthSigningAlg, cancellationToken).ConfigureAwait(false);

        //var tokenLifetime = scope.Contains("uma_protection", StringComparison.Ordinal) ? client.TokenLifetime
        var accessToken = handler.CreateEncodedJwt(
            issuerName,
            client.ClientId,
            new ClaimsIdentity(enumerable),
            DateTime.UtcNow,
            DateTime.UtcNow.Add(client.TokenLifetime),
            DateTime.UtcNow,
            signingCredentials);

        // 3. Construct the refresh token.
        return new GrantedToken
        {
            Id = Id.Create(),
            AccessToken = accessToken,
            RefreshToken = scopes.Contains(CoreConstants.Offline) ? Id.Create() : null,
            ExpiresIn = (int)client.TokenLifetime.TotalSeconds,
            TokenType = CoreConstants.StandardTokenTypes.Bearer,
            CreateDateTime = DateTimeOffset.UtcNow,
            // IDS
            Scope = scopeString,
            UserInfoPayLoad = userInformationPayload,
            IdTokenPayLoad = idTokenPayload,
            ClientId = client.ClientId
        };
    }
}