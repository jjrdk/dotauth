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

namespace SimpleAuth.JwtToken
{
    using Exceptions;
    using Extensions;
    using Microsoft.IdentityModel.Tokens;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class JwtGenerator
    {
        private readonly IScopeStore _scopeRepository;
        private readonly IJwksStore _jwksStore;
        private readonly IClientStore _clientRepository;

        private readonly Dictionary<string, Func<string, string>> _mappingJwsAlgToHashingFunctions =
            new Dictionary<string, Func<string, string>>
            {
                {SecurityAlgorithms.EcdsaSha256, HashWithSha256},
                {SecurityAlgorithms.EcdsaSha384, HashWithSha384},
                {SecurityAlgorithms.EcdsaSha512, HashWithSha512},
                {SecurityAlgorithms.HmacSha256, HashWithSha256},
                {SecurityAlgorithms.HmacSha384, HashWithSha384},
                {SecurityAlgorithms.HmacSha512, HashWithSha512},
                {SecurityAlgorithms.RsaSsaPssSha256, HashWithSha256},
                {SecurityAlgorithms.RsaSsaPssSha384, HashWithSha384},
                {SecurityAlgorithms.RsaSsaPssSha512, HashWithSha512},
                {SecurityAlgorithms.RsaSha256, HashWithSha256},
                {SecurityAlgorithms.RsaSha384, HashWithSha384},
                {SecurityAlgorithms.RsaSha512, HashWithSha512}
            };

        public JwtGenerator(IClientStore clientRepository, IScopeStore scopeRepository, IJwksStore jwksStore)
        {
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _jwksStore = jwksStore;
        }

        public JwtPayload UpdatePayloadDate(JwtPayload jwsPayload, TimeSpan? duration)
        {
            if (jwsPayload == null)
            {
                throw new ArgumentNullException(nameof(jwsPayload));
            }

            var timeKeyValuePair = GetExpirationAndIssuedTime(duration);
            var expirationInSeconds = timeKeyValuePair.Key;
            var issuedAtTime = timeKeyValuePair.Value;
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
            var timeKeyValuePair = GetExpirationAndIssuedTime(client?.TokenLifetime);
            var expirationInSeconds = timeKeyValuePair.Key;
            var issuedAtTime = timeKeyValuePair.Value;

            var key = await _jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg, cancellationToken).ConfigureAwait(false)
                ?? await _jwksStore.GetDefaultSigningKey(cancellationToken).ConfigureAwait(false);
            //client.JsonWebKeys.GetSigningCredentials(client.IdTokenSignedResponseAlg).First();

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

            if (additionalClaims != null)
            {
                payload.AddClaims(additionalClaims);
            }

            return token;
        }

        public async Task<JwtPayload> GenerateIdTokenPayloadForScopes(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var result = new JwtPayload();
            await FillInIdentityTokenClaims(
                    result,
                    authorizationParameter,
                    new List<ClaimParameter>(),
                    claimsPrincipal,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            await FillInResourceOwnerClaimsFromScopes(
                    result,
                    authorizationParameter,
                    claimsPrincipal,
                    cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<JwtPayload> GenerateFilteredIdTokenPayload(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var result = new JwtPayload();
            await FillInIdentityTokenClaims(
                    result,
                    authorizationParameter,
                    claimParameters,
                    claimsPrincipal,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            FillInResourceOwnerClaimsByClaimsParameter(
                result,
                claimParameters,
                claimsPrincipal,
                authorizationParameter);
            return result;
        }

        public async Task<JwtPayload> GenerateUserInfoPayloadForScope(
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter,
            CancellationToken cancellationToken)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var result = new JwtPayload();
            await FillInResourceOwnerClaimsFromScopes(
                    result,
                    authorizationParameter,
                    claimsPrincipal,
                    cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public JwtPayload GenerateFilteredUserInfoPayload(
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            if (claimsPrincipal?.Identity == null || !claimsPrincipal.IsAuthenticated())
            {
                throw new ArgumentNullException(nameof(claimParameters));
            }

            var result = new JwtPayload();
            FillInResourceOwnerClaimsByClaimsParameter(
                result,
                claimParameters,
                claimsPrincipal,
                authorizationParameter);
            return result;
        }

        public void FillInOtherClaimsIdentityTokenPayload(
            JwtPayload jwsPayload,
            string authorizationCode,
            string accessToken,
            Client client)
        {
            var signedAlg = client.IdTokenSignedResponseAlg;
            if (signedAlg == null || signedAlg == SecurityAlgorithms.None)
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
            jwsPayload.Add(Shared.OpenIdClaimTypes.Subject, subject);

            if (authorizationParameter != null && !string.IsNullOrWhiteSpace(authorizationParameter.Scope))
            {
                var scopes = authorizationParameter.Scope.ParseScopes();
                var claims = await this.GetClaimsFromRequestedScopes(claimsPrincipal, cancellationToken, scopes).ConfigureAwait(false);
                foreach (var claim in claims.GroupBy(c => c.Type).Where(x => x.Key != Shared.OpenIdClaimTypes.Subject))
                {
                    jwsPayload.Add(claim.Key, string.Join(" ", claim.Select(c => c.Value)));
                }
            }

            // 2. Fill-in the other claims
            return jwsPayload;
        }

        private void FillInResourceOwnerClaimsByClaimsParameter(
            JwtPayload jwsPayload,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            var state = authorizationParameter == null ? string.Empty : authorizationParameter.State;

            // 1. Fill-In the subject - set the subject as an essential claim
            if (claimParameters.All(c => c.Name != Shared.OpenIdClaimTypes.Subject))
            {
                var essentialSubjectClaimParameter = new ClaimParameter
                {
                    Name = Shared.OpenIdClaimTypes.Subject,
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
                return;
            }

            var resourceOwnerClaimParameters = claimParameters
                .Where(c => Shared.JwtConstants.AllStandardResourceOwnerClaimNames.Contains(c.Name))
                .ToList();
            if (resourceOwnerClaimParameters.Any())
            {
                var requestedClaimNames = resourceOwnerClaimParameters.Select(r => r.Name).ToArray();
                var resourceOwnerClaims = GetClaims(claimsPrincipal, requestedClaimNames);
                foreach (var resourceOwnerClaimParameter in resourceOwnerClaimParameters)
                {
                    var resourceOwnerClaim =
                        resourceOwnerClaims.FirstOrDefault(c => c.Type == resourceOwnerClaimParameter.Name);
                    var resourceOwnerClaimValue = resourceOwnerClaim == null ? string.Empty : resourceOwnerClaim.Value;
                    var isClaimValid = ValidateClaimValue(resourceOwnerClaimValue, resourceOwnerClaimParameter);
                    if (!isClaimValid)
                    {
                        throw new SimpleAuthExceptionWithState(
                            ErrorCodes.InvalidGrant,
                            string.Format(ErrorDescriptions.TheClaimIsNotValid, resourceOwnerClaimParameter.Name),
                            state);
                    }

                    jwsPayload.Add(resourceOwnerClaim.Type, resourceOwnerClaim.Value);
                }
            }
        }

        private async Task FillInIdentityTokenClaims(
            JwtPayload jwsPayload,
            AuthorizationParameter authorizationParameter,
            List<ClaimParameter> claimParameters,
            ClaimsPrincipal claimsPrincipal,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var nonce = authorizationParameter.Nonce;
            var state = authorizationParameter.State;
            var clientId = authorizationParameter.ClientId;
            var maxAge = authorizationParameter.MaxAge;
            var amrValues = authorizationParameter.AmrValues;
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

            var timeKeyValuePair = GetExpirationAndIssuedTime(cl?.TokenLifetime);
            var expirationInSeconds = timeKeyValuePair.Key;
            var issuedAtTime = timeKeyValuePair.Value;
            var acrValues = CoreConstants.StandardArcParameterNames._openIdCustomAuthLevel + ".password=1";
            var amr = new[] { "password" };
            if (amrValues != null)
            {
                amr = amrValues.ToArray();
            }

            var azp = string.Empty;

            var clients = await _clientRepository.GetAll(cancellationToken).ConfigureAwait(false);
            var audiences = (from client in clients
                             let isClientSupportIdTokenResponseType = client.CheckResponseTypes(ResponseTypeNames.IdToken)
                             where isClientSupportIdTokenResponseType || client.ClientId == authorizationParameter.ClientId
                             select client.ClientId).ToList();

            // The identity token can be reused by the identity server.
            if (!string.IsNullOrWhiteSpace(issuerName))
            {
                audiences.Add(issuerName);
            }

            var authenticationInstant =
                claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
            var authenticationInstantValue = authenticationInstant == null ? string.Empty : authenticationInstant.Value;

            if (issuerClaimParameter != null)
            {
                var issuerIsValid = ValidateClaimValue(issuerName, issuerClaimParameter);
                if (!issuerIsValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Issuer),
                        state);
                }
            }

            if (audiences.Count > 1 || (audiences.Count == 1 && audiences[0] != clientId))
            {
                azp = clientId;
            }

            if (audiencesClaimParameter != null)
            {
                var audiencesIsValid = ValidateClaimValues(audiences.ToArray(), audiencesClaimParameter);
                if (!audiencesIsValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Audiences),
                        state);
                }
            }

            if (expirationTimeClaimParameter != null)
            {
                var expirationInSecondsIsValid = ValidateClaimValue(
                    expirationInSeconds.ToString(CultureInfo.InvariantCulture),
                    expirationTimeClaimParameter);
                if (!expirationInSecondsIsValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.ExpirationTime),
                        state);
                }
            }

            if (issuedAtTimeClaimParameter != null)
            {
                var issuedAtTimeIsValid = ValidateClaimValue(issuedAtTime.ToString(), issuedAtTimeClaimParameter);
                if (!issuedAtTimeIsValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Iat),
                        state);
                }
            }

            if (authenticationTimeParameter != null)
            {
                var isAuthenticationTimeValid = ValidateClaimValue(
                    authenticationInstantValue,
                    authenticationTimeParameter);
                if (!isAuthenticationTimeValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.AuthenticationTime),
                        state);
                }
            }

            if (acrParameter != null)
            {
                var isAcrParameterValid = ValidateClaimValue(acrValues, acrParameter);
                if (!isAcrParameterValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Acr),
                        state);
                }
            }

            if (nonceParameter != null)
            {
                var isNonceParameterValid = ValidateClaimValue(nonce, nonceParameter);
                if (!isNonceParameterValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Nonce),
                        state);
                }
            }

            if (amrParameter != null)
            {
                var isAmrParameterValid = ValidateClaimValues(amr, amrParameter);
                if (!isAmrParameterValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Amr),
                        state);
                }
            }

            // Fill-in the AZP parameter
            if (azpParameter != null)
            {
                var isAzpParameterValid = ValidateClaimValue(azp, azpParameter);
                if (!isAzpParameterValid)
                {
                    throw new SimpleAuthExceptionWithState(
                        ErrorCodes.InvalidGrant,
                        string.Format(ErrorDescriptions.TheClaimIsNotValid, StandardClaimNames.Azp),
                        state);
                }
            }

            jwsPayload.Add(StandardClaimNames.Issuer, issuerName);
            jwsPayload.Add(StandardClaimNames.Audiences, audiences.ToArray());
            jwsPayload.Add(StandardClaimNames.ExpirationTime, expirationInSeconds);
            jwsPayload.Add(StandardClaimNames.Iat, issuedAtTime);

            // Set the auth_time if it's requested as an essential claim OR the max_age request is specified
            if (((authenticationTimeParameter != null && authenticationTimeParameter.Essential)
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
        }

        private bool ValidateClaimValue(object claimValue, ClaimParameter claimParameter)
        {
            if (claimParameter.EssentialParameterExist
                && (claimValue == null || string.IsNullOrWhiteSpace(claimValue.ToString()))
                && claimParameter.Essential)
            {
                return false;
            }

            if (claimParameter.ValueParameterExist && claimValue.ToString() != claimParameter.Value)
            {
                return false;
            }

            if (claimParameter.ValuesParameterExist
                && claimParameter.Values != null
                && claimParameter.Values.Contains(claimValue))
            {
                return false;
            }

            return true;
        }

        private bool ValidateClaimValues(string[] claimValues, ClaimParameter claimParameter)
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

            if (claimParameter.ValuesParameterExist
                && claimParameter.Values != null
                && (claimValues == null || !claimParameter.Values.All(claimValues.Contains)))
            {
                return false;
            }

            return true;
        }

        private async Task<List<Claim>> GetClaimsFromRequestedScopes(
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken,
            params string[] scopes)
        {
            var returnedScopes = await _scopeRepository.SearchByNames(cancellationToken, scopes).ConfigureAwait(false);
            var claims = returnedScopes.SelectMany(x => GetClaims(claimsPrincipal, x.Claims.ToArray())).ToList();
            var claimsFromRequestedScopes = GetUniqueClaimsFromRequestedScopes(claims);

            return claimsFromRequestedScopes;
        }

        private static List<Claim> GetUniqueClaimsFromRequestedScopes(List<Claim> claims)
        {
            var uniqueClaims = new List<Claim>();
            foreach (var claim in claims)
            {
                if (!uniqueClaims.Any(cfr => cfr.Type.Equals(claim.Type) && cfr.Value.Equals(claim.Value)))
                {
                    uniqueClaims.Add(claim);
                }
            }

            return uniqueClaims;
        }

        private IList<Claim> GetClaims(ClaimsPrincipal claimsPrincipal, params string[] claims)
        {
            var openIdClaims = MapToOpenIdClaims(claimsPrincipal.Claims);
            return openIdClaims.Where(oc => claims.Contains(oc.Type)).ToArray();
        }

        private IEnumerable<Claim> MapToOpenIdClaims(IEnumerable<Claim> claims)
        {
            return claims.Select(
                claim => new Claim(
                    Shared.JwtConstants.MapWifClaimsToOpenIdClaims.ContainsKey(claim.Type)
                        ? Shared.JwtConstants.MapWifClaimsToOpenIdClaims[claim.Type]
                        : claim.Type,
                    claim.Value));
        }

        private KeyValuePair<double, double> GetExpirationAndIssuedTime(TimeSpan? duration)
        {
            var currentDateTime = DateTimeOffset.UtcNow;
            var expiredDateTime = currentDateTime.Add(duration ?? TimeSpan.Zero);
            var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
            var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
            return new KeyValuePair<double, double>(expirationInSeconds, iatInSeconds);
        }

        private static string HashWithSha256(string parameter)
        {
            var sha256 = SHA256.Create();
            return GetFirstPart(parameter, sha256);
        }

        private static string HashWithSha384(string parameter)
        {
            var sha384 = SHA384.Create();
            return GetFirstPart(parameter, sha384);
        }

        private static string HashWithSha512(string parameter)
        {
            var sha512 = SHA512.Create();
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
            return new[] { firstHalf, secondHalf };
        }
    }
}
