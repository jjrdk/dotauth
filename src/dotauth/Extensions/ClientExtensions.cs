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

namespace DotAuth.Extensions;

using System.IdentityModel.Tokens.Jwt;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;

internal static class ClientExtensions
{
    public static async Task<TokenValidationParameters> CreateValidationParameters(this Client client, IJwksStore jwksStore, string? audience = null, string? issuer = null, CancellationToken cancellationToken = default)
    {
        var signingKeys = await client.GetSigningCredentials(jwksStore, cancellationToken).ConfigureAwait(false);
        var encryptionKeys = client.JsonWebKeys.GetEncryptionKeys().ToArray();
        if (encryptionKeys.Length == 0 && client.IdTokenEncryptedResponseAlg != null)
        {
            var key = await jwksStore.GetEncryptionKey(client.IdTokenEncryptedResponseAlg, cancellationToken).ConfigureAwait(false);

            encryptionKeys = [key];
        }
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKeys = signingKeys.Select(x => x!.Key).ToArray(),
            TokenDecryptionKeys = encryptionKeys
        };
        if (audience != null)
        {
            parameters.ValidAudience = audience;
        }
        else
        {
            parameters.ValidateAudience = false;
        }
        if (issuer != null)
        {
            parameters.ValidIssuer = issuer;
        }
        else
        {
            parameters.ValidateIssuer = false;
        }

        return parameters;
    }

    public static async Task<string?> GenerateIdToken(
        this IClientStore clientStore,
        string clientId,
        JwtPayload jwsPayload,
        IJwksStore jwksStore,
        CancellationToken cancellationToken)
    {
        var client = await clientStore.GetById(clientId, cancellationToken).ConfigureAwait(false);
        return client == null
            ? null
            : await GenerateIdToken(client, jwsPayload, jwksStore, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<SigningCredentials?[]> GetSigningCredentials(
        this Client client,
        IJwksStore jwksStore,
        CancellationToken cancellationToken = default)
    {
        var signingKeys = client.JsonWebKeys?.Keys.Where(key => key.Use == JsonWebKeyUseNames.Sig)
            .Select(key => new SigningCredentials(key, key.Alg))
            .ToArray();
        if (signingKeys?.Length != 0)
        {
            return signingKeys!;
        }

        var keys = await (client.IdTokenSignedResponseAlg == null
                ? jwksStore.GetDefaultSigningKey(cancellationToken)
                : jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg, cancellationToken))
            .ConfigureAwait(false);

        return [keys];
    }

    public static async Task<string> GenerateIdToken(
        this Client client,
        JwtPayload jwsPayload,
        IJwksStore jwksStore,
        CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var signingCredentials =
            await client.GetSigningCredentials(jwksStore, cancellationToken).ConfigureAwait(false);
        var claimsIdentity = new ClaimsIdentity(jwsPayload.Claims);
            //.Where(c => !string.IsNullOrWhiteSpace(c.Value)).Where(c => OpenIdClaimTypes.All.Contains(c.Type)));
        var now = DateTime.UtcNow;
        var jwt = handler.CreateEncodedJwt(
            jwsPayload.Iss,
            client.ClientId,
            claimsIdentity,
            now,
            now.Add(client.TokenLifetime),
            now,
            signingCredentials[0]);

        return jwt;
    }
}
