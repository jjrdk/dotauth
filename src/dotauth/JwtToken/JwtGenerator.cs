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

namespace DotAuth.JwtToken;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

internal sealed class JwtGenerator
{
    private readonly IScopeStore _scopeRepository;
    private readonly IJwksStore _jwksStore;
    private readonly ILogger _logger;
    private readonly IClientStore _clientRepository;

    private readonly Dictionary<string, Func<string, string>> _mappingJwsAlgToHashingFunctions =
        new()
        {
            { SecurityAlgorithms.EcdsaSha256, HashWithSha256 },
            { SecurityAlgorithms.EcdsaSha384, HashWithSha384 },
            { SecurityAlgorithms.EcdsaSha512, HashWithSha512 },
            { SecurityAlgorithms.HmacSha256, HashWithSha256 },
            { SecurityAlgorithms.HmacSha384, HashWithSha384 },
            { SecurityAlgorithms.HmacSha512, HashWithSha512 },
            { SecurityAlgorithms.RsaSsaPssSha256, HashWithSha256 },
            { SecurityAlgorithms.RsaSsaPssSha384, HashWithSha384 },
            { SecurityAlgorithms.RsaSsaPssSha512, HashWithSha512 },
            { SecurityAlgorithms.RsaSha256, HashWithSha256 },
            { SecurityAlgorithms.RsaSha384, HashWithSha384 },
            { SecurityAlgorithms.RsaSha512, HashWithSha512 }
        };

    public JwtGenerator(IClientStore clientRepository, IScopeStore scopeRepository, IJwksStore jwksStore, ILogger logger)
    {
        _clientRepository = clientRepository;
        _scopeRepository = scopeRepository;
        _jwksStore = jwksStore;
        _logger = logger;
    }

    public static JwtPayload UpdatePayloadDate(JwtPayload jwsPayload, TimeSpan? duration)
    {
        if (jwsPayload == null)
        {
            throw new ArgumentNullException(nameof(jwsPayload));
        }

        var (expirationInSeconds, issuedAtTime) = GetExpirationAndIssuedTime(duration);
        jwsPayload.Remove(StandardClaimNames.Iat);
        jwsPayload.Remove(StandardClaimNames.ExpirationTime);
        jwsPayload.AddClaim(
            new Claim(
                StandardClaimNames.Iat,
                issuedAtTime.ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Double));
        jwsPayload.AddClaim(
            new Claim(
                StandardClaimNames.ExpirationTime,
                expirationInSeconds.ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Double));
        return jwsPayload;
    }

    public async Task<JwtSecurityToken> GenerateAccessToken(
        Client client,
        IEnumerable<string> scopes,
        string issuerName,
        CancellationToken cancellationToken = default,
        params Claim[] additionalClaims)
    {
        var (expirationInSeconds, issuedAtTime) = GetExpirationAndIssuedTime(client.TokenLifetime);

        var key = string.IsNullOrWhiteSpace(client.IdTokenSignedResponseAlg)
            ? await _jwksStore.GetDefaultSigningKey(cancellationToken).ConfigureAwait(false)
            : await _jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg, cancellationToken)
                .ConfigureAwait(false);

        var jwtHeader = new JwtHeader(key);
        var payload = new JwtPayload(
            new[]
            {
                new Claim(StandardClaimNames.Audiences, client.ClientId),
                new Claim(StandardClaimNames.Issuer, issuerName),
                new Claim(
                    StandardClaimNames.ExpirationTime,
                    expirationInSeconds.ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Double),
                new Claim(
                    StandardClaimNames.Iat,
                    issuedAtTime.ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Double),
                new Claim(StandardClaimNames.Scopes, string.Join(" ", scopes))
            });
        var token = new JwtSecurityToken(jwtHeader, payload);

        payload.AddClaims(additionalClaims);

        return token;
    }

    public async Task<Option<JwtPayload>> GenerateIdTokenPayloadForScopes(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        if (claimsPrincipal.Identity == null || !claimsPrincipal.IsAuthenticated())
        {
            throw new ArgumentNullException(nameof(claimsPrincipal));
        }

        var result = await FillInIdentityTokenClaims(
                new JwtPayload(),
                authorizationParameter,
                [],
                claimsPrincipal,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (result is not Option<JwtPayload>.Result r)
        {
            return result;
        }
        var payload = await FillInResourceOwnerClaimsFromScopes(
                r.Item,
                authorizationParameter,
                claimsPrincipal,
                cancellationToken)
            .ConfigureAwait(false);
        return new Option<JwtPayload>.Result(payload);
    }

    public async Task<Option<JwtPayload>> GenerateFilteredIdTokenPayload(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter,
        ClaimParameter[] claimParameters,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        if (claimsPrincipal.Identity == null || !claimsPrincipal.IsAuthenticated())
        {
            throw new ArgumentNullException(nameof(claimsPrincipal));
        }

        var result = await FillInIdentityTokenClaims(
                new JwtPayload(),
                authorizationParameter,
                claimParameters,
                claimsPrincipal,
                issuerName,
                cancellationToken)
            .ConfigureAwait(false);
        if (result is not Option<JwtPayload>.Result r)
        {
            return result;
        }

        return FillInResourceOwnerClaimsByClaimsParameter(
            r.Item,
            claimParameters,
            claimsPrincipal,
            authorizationParameter);
    }

    public async Task<JwtPayload> GenerateUserInfoPayloadForScope(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter,
        CancellationToken cancellationToken)
    {
        if (claimsPrincipal.Identity == null || !claimsPrincipal.IsAuthenticated())
        {
            throw new ArgumentNullException(nameof(claimsPrincipal));
        }

        return await FillInResourceOwnerClaimsFromScopes(
                new JwtPayload(),
                authorizationParameter,
                claimsPrincipal,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Option<JwtPayload> GenerateFilteredUserInfoPayload(
        ClaimParameter[] claimParameters,
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter)
    {
        if (claimsPrincipal.Identity == null || !claimsPrincipal.IsAuthenticated())
        {
            throw new ArgumentException(Strings.IdentityMissing, nameof(claimParameters));
        }

        return FillInResourceOwnerClaimsByClaimsParameter(
            new JwtPayload(),
            claimParameters,
            claimsPrincipal,
            authorizationParameter);
    }

    public void FillInOtherClaimsIdentityTokenPayload(
        JwtPayload jwsPayload,
        string authorizationCode,
        string accessToken,
        Client client)
    {
        var signedAlg = client.IdTokenSignedResponseAlg;
        if (signedAlg is null or SecurityAlgorithms.None)
        {
            return;
        }

        if (!_mappingJwsAlgToHashingFunctions.ContainsKey(signedAlg))
        {
            throw new InvalidOperationException($"the alg {signedAlg} is not supported");
        }

        var callback = _mappingJwsAlgToHashingFunctions[signedAlg];
        if (!string.IsNullOrWhiteSpace(authorizationCode))
        {
            var hashingAuthorizationCode = callback(authorizationCode);
            jwsPayload.Add(StandardClaimNames.CHash, hashingAuthorizationCode);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var hashingAccessToken = callback(accessToken);
            jwsPayload.Add(StandardClaimNames.AtHash, hashingAccessToken);
        }
    }

    private async Task<JwtPayload> FillInResourceOwnerClaimsFromScopes(
        JwtPayload jwsPayload,
        AuthorizationParameter authorizationParameter,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        // 1. Fill-in the subject claim
        var subject = claimsPrincipal.GetSubject();
        jwsPayload.Add(OpenIdClaimTypes.Subject, subject);

        if (string.IsNullOrWhiteSpace(authorizationParameter.Scope))
        {
            return jwsPayload;
        }

        var scopes = authorizationParameter.Scope.ParseScopes();
        var claims = await GetClaimsFromRequestedScopes(claimsPrincipal, cancellationToken, scopes)
            .ConfigureAwait(false);
        foreach (var claim in claims
                     .GroupBy(c => c.Type)
                     .Where(x => x.Key != OpenIdClaimTypes.Subject))
        {
            jwsPayload.Add(claim.Key, string.Join(" ", claim.Select(c => c.Value)));
        }

        return jwsPayload;
    }

    private Option<JwtPayload> FillInResourceOwnerClaimsByClaimsParameter(
        JwtPayload jwsPayload,
        ClaimParameter[] claimParameters,
        ClaimsPrincipal claimsPrincipal,
        AuthorizationParameter authorizationParameter)
    {
        // 1. Fill-In the subject - set the subject as an essential claim
        if (claimParameters.All(c => c.Name != OpenIdClaimTypes.Subject))
        {
            var essentialSubjectClaimParameter = new ClaimParameter
            {
                Name = OpenIdClaimTypes.Subject,
                Parameters = new Dictionary<string, object>
                {
                    {CoreConstants.StandardClaimParameterValueNames.EssentialName, true}
                }
            };

            claimParameters.Add(essentialSubjectClaimParameter);
        }

        // 2. Fill-In all the other resource owner claims
        if (!claimParameters.Any())
        {
            return new Option<JwtPayload>.Result(jwsPayload);
        }

        var resourceOwnerClaimParameters = claimParameters
            .Where(c => Shared.JwtConstants.AllStandardResourceOwnerClaimNames.Contains(c.Name))
            .ToArray();
        if (resourceOwnerClaimParameters.Length == 0)
        {
            return new Option<JwtPayload>.Result(jwsPayload);
        }

        var requestedClaimNames = resourceOwnerClaimParameters.Select(r => r.Name).ToArray();
        var resourceOwnerClaims = GetClaims(claimsPrincipal, requestedClaimNames);
        foreach (var resourceOwnerClaimParameter in resourceOwnerClaimParameters)
        {
            var resourceOwnerClaim =
                resourceOwnerClaims.FirstOrDefault(c => c.Type == resourceOwnerClaimParameter.Name);
            var isClaimValid = ValidateClaimValue(resourceOwnerClaim?.Value, resourceOwnerClaimParameter);
            if (!isClaimValid)
            {
                _logger.LogError(Strings.TheClaimIsNotValid, resourceOwnerClaimParameter.Name);
                return new Option<JwtPayload>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidGrant,
                        Detail = string.Format(Strings.TheClaimIsNotValid, resourceOwnerClaimParameter.Name),
                        Status = HttpStatusCode.BadRequest
                    },
                    authorizationParameter.State);
            }

            jwsPayload.Add(resourceOwnerClaim!.Type, resourceOwnerClaim.Value);
        }

        return new Option<JwtPayload>.Result(jwsPayload);
    }

    private async Task<Option<JwtPayload>> FillInIdentityTokenClaims(
        JwtPayload jwsPayload,
        AuthorizationParameter authorizationParameter,
        ClaimParameter[] claimParameters,
        ClaimsPrincipal claimsPrincipal,
        string? issuerName,
        CancellationToken cancellationToken)
    {
        var nonce = authorizationParameter.Nonce;
        var state = authorizationParameter.State;
        var clientId = authorizationParameter.ClientId!;
        var maxAge = authorizationParameter.MaxAge;
        //var amrValues = authorizationParameter.AmrValues;
        var cl = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
        var issuerClaimParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Issuer);
        var audiencesClaimParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Audiences);
        var expirationTimeClaimParameter =
            claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.ExpirationTime);
        var issuedAtTimeClaimParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Iat);
        var authenticationTimeParameter =
            claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.AuthenticationTime);
        var nonceParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Nonce);
        var acrParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Acr);
        var amrParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Amr);
        var azpParameter = claimParameters.FirstOrDefault(c => c.Name == StandardClaimNames.Azp);

        var (expirationInSeconds, issuedAtTime) = GetExpirationAndIssuedTime(cl?.TokenLifetime);
        const string acrValues = CoreConstants.StandardArcParameterNames.OpenIdCustomAuthLevel + ".password=1";
        var amr = new[] { "password" };

        var azp = string.Empty;

        var clients = await _clientRepository.GetAll(cancellationToken).ConfigureAwait(false);
        var stringComparer = StringComparer.OrdinalIgnoreCase;
        var audiences = clients
            .Select(client => (client.ClientId, client.CheckResponseTypes(ResponseTypeNames.IdToken)))
            .Where(
                t => t.Item2
                     || t.ClientId == authorizationParameter.ClientId)
            .Select(t => t.ClientId)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(stringComparer).ToArray();

        // The identity token can be reused by the identity server.
        if (!string.IsNullOrWhiteSpace(issuerName) && !audiences.Contains(issuerName, stringComparer))
        {
            audiences = audiences.Add(issuerName);
        }

        var authenticationInstant = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.AuthenticationInstant).MaxBy(x => x.Value);
        var authenticationInstantValue = authenticationInstant == null ? string.Empty : authenticationInstant.Value;

        if (issuerClaimParameter != null)
        {
            var issuerIsValid = ValidateClaimValue(issuerName, issuerClaimParameter);
            if (!issuerIsValid)
            {
                return CreateClaimError(StandardClaimNames.Issuer, state);
            }
        }

        if (audiences.Length > 1 || (audiences.Length == 1 && audiences[0] != clientId))
        {
            azp = clientId;
        }

        if (audiencesClaimParameter != null)
        {
            var audiencesIsValid = ValidateClaimValues(audiences.ToArray(), audiencesClaimParameter);
            if (!audiencesIsValid)
            {
                return CreateClaimError(StandardClaimNames.Audiences, state);
            }
        }

        if (expirationTimeClaimParameter != null)
        {
            var expirationInSecondsIsValid = ValidateClaimValue(
                expirationInSeconds.ToString(CultureInfo.InvariantCulture),
                expirationTimeClaimParameter);
            if (!expirationInSecondsIsValid)
            {
                return CreateClaimError(StandardClaimNames.ExpirationTime, state);
            }
        }

        if (issuedAtTimeClaimParameter != null)
        {
            var issuedAtTimeIsValid = ValidateClaimValue(issuedAtTime.ToString(CultureInfo.InvariantCulture), issuedAtTimeClaimParameter);
            if (!issuedAtTimeIsValid)
            {
                return CreateClaimError(StandardClaimNames.Iat, state);
            }
        }

        if (authenticationTimeParameter != null)
        {
            var isAuthenticationTimeValid = ValidateClaimValue(
                authenticationInstantValue,
                authenticationTimeParameter);
            if (!isAuthenticationTimeValid)
            {
                return CreateClaimError(StandardClaimNames.AuthenticationTime, state);
            }
        }

        if (acrParameter != null)
        {
            var isAcrParameterValid = ValidateClaimValue(acrValues, acrParameter);
            if (!isAcrParameterValid)
            {
                return CreateClaimError(StandardClaimNames.Acr, state);
            }
        }

        if (nonceParameter != null)
        {
            var isNonceParameterValid = ValidateClaimValue(nonce, nonceParameter);
            if (!isNonceParameterValid)
            {
                return CreateClaimError(StandardClaimNames.Nonce, state);
            }
        }

        if (amrParameter != null)
        {
            var isAmrParameterValid = ValidateClaimValues(amr, amrParameter);
            if (!isAmrParameterValid)
            {
                return CreateClaimError(StandardClaimNames.Amr, state);
            }
        }

        // Fill-in the AZP parameter
        if (azpParameter != null)
        {
            var isAzpParameterValid = ValidateClaimValue(azp, azpParameter);
            if (!isAzpParameterValid)
            {
                return CreateClaimError(StandardClaimNames.Azp, state);
            }
        }

        jwsPayload.Add(StandardClaimNames.Issuer, issuerName);
        jwsPayload.Add(StandardClaimNames.Audiences, audiences.ToArray());
        jwsPayload.Add(StandardClaimNames.ExpirationTime, expirationInSeconds);
        jwsPayload.Add(StandardClaimNames.Iat, issuedAtTime);

        // Set the auth_time if it's requested as an essential claim OR the max_age request is specified
        if ((authenticationTimeParameter is { Essential: true }
             || !maxAge.Equals(default))
            && !string.IsNullOrWhiteSpace(authenticationInstantValue))
        {
            jwsPayload.Add(StandardClaimNames.AuthenticationTime, double.Parse(authenticationInstantValue));
        }

        if (!string.IsNullOrWhiteSpace(nonce))
        {
            jwsPayload.Add(StandardClaimNames.Nonce, nonce);
        }

        jwsPayload.Add(StandardClaimNames.Acr, acrValues);
        jwsPayload.Add(StandardClaimNames.Amr, amr);
        if (!string.IsNullOrWhiteSpace(azp))
        {
            jwsPayload.Add(StandardClaimNames.Azp, azp);
        }

        return new Option<JwtPayload>.Result(jwsPayload);
    }

    private Option<JwtPayload> CreateClaimError(string claimName, string? state)
    {
        var message = string.Format(Strings.TheClaimIsNotValid, claimName);
        _logger.LogError(Strings.TheClaimIsNotValid, claimName);
        return new Option<JwtPayload>.Error(
            new ErrorDetails { Title = ErrorCodes.InvalidGrant, Detail = message, Status = HttpStatusCode.BadRequest },
            state);
    }

    private static bool ValidateClaimValue(object? claimValue, ClaimParameter claimParameter)
    {
        if (claimParameter.EssentialParameterExist
            && (claimValue == null || string.IsNullOrWhiteSpace(claimValue.ToString()))
            && claimParameter.Essential)
        {
            return false;
        }

        if (claimParameter.ValueParameterExist && claimValue!.ToString() != claimParameter.Value)
        {
            return false;
        }

        return !claimParameter.ValuesParameterExist
               || claimParameter.Values == null
               || !claimParameter.Values.Contains(claimValue);
    }

    private static bool ValidateClaimValues(string[]? claimValues, ClaimParameter claimParameter)
    {
        if (claimParameter.EssentialParameterExist
            && (claimValues == null || claimValues.Any())
            && claimParameter.Essential)
        {
            return false;
        }

        if (claimParameter.ValueParameterExist
            && (claimValues == null || !claimValues.Contains(claimParameter.Value)))
        {
            return false;
        }

        return !claimParameter.ValuesParameterExist
               || claimParameter.Values == null
               || (claimValues != null && claimParameter.Values.All(claimValues.Contains));
    }

    private async Task<List<Claim>> GetClaimsFromRequestedScopes(
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken,
        params string[] scopes)
    {
        var returnedScopes = await _scopeRepository.SearchByNames(cancellationToken, scopes).ConfigureAwait(false);
        var claims = returnedScopes.SelectMany(x => GetClaims(claimsPrincipal, x.Claims.ToArray()));
        return claims.Distinct(new ClaimEqualityComparer()).ToList();
    }

    private static IList<Claim> GetClaims(ClaimsPrincipal claimsPrincipal, params string[] claims)
    {
        var openIdClaims = MapToOpenIdClaims(claimsPrincipal.Claims);
        return openIdClaims.Where(oc => claims.Contains(oc.Type)).ToArray();
    }

    private static IEnumerable<Claim> MapToOpenIdClaims(IEnumerable<Claim> claims)
    {
        return claims.Select(
            claim => new Claim(
                Shared.JwtConstants.MapWifClaimsToOpenIdClaims.ContainsKey(claim.Type)
                    ? Shared.JwtConstants.MapWifClaimsToOpenIdClaims[claim.Type]
                    : claim.Type,
                claim.Value));
    }

    private static KeyValuePair<double, double> GetExpirationAndIssuedTime(TimeSpan? duration)
    {
        var currentDateTime = DateTimeOffset.UtcNow;
        var expiredDateTime = currentDateTime.Add(duration ?? TimeSpan.Zero);
        var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
        var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
        return new KeyValuePair<double, double>(expirationInSeconds, iatInSeconds);
    }

    private static string HashWithSha256(string parameter)
    {
        using var sha256 = SHA256.Create();
        return GetFirstPart(parameter, sha256);
    }

    private static string HashWithSha384(string parameter)
    {
        using var sha384 = SHA384.Create();
        return GetFirstPart(parameter, sha384);
    }

    private static string HashWithSha512(string parameter)
    {
        using var sha512 = SHA512.Create();
        return GetFirstPart(parameter, sha512);
    }

    private static string GetFirstPart(string parameter, HashAlgorithm alg)
    {
        var hashingResultBytes = alg.ComputeHash(Encoding.UTF8.GetBytes(parameter));
        var split = SplitByteArrayInHalf(hashingResultBytes);
        var firstPart = split[0];
        return firstPart.ToBase64Simplified();
    }

    private static byte[][] SplitByteArrayInHalf(byte[] arr)
    {
        var halfIndex = arr.Length / 2;
        var firstHalf = new byte[halfIndex];
        var secondHalf = new byte[halfIndex];
        Buffer.BlockCopy(arr, 0, firstHalf, 0, halfIndex);
        Buffer.BlockCopy(arr, halfIndex, secondHalf, 0, halfIndex);
        return [firstHalf, secondHalf];
    }
}