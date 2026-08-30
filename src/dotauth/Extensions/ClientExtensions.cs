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
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;

internal static class ClientExtensions
{
     /// <summary>How long a fetched client JWKS stays in the cache before it is re-fetched.</summary>
    private static readonly TimeSpan JwksCacheTtl = TimeSpan.FromMinutes(5);

     /// <summary>Per-URI cache entry: the key set plus the wall-clock time it was fetched.</summary>
    private readonly record struct CacheEntry(JsonWebKeySet Keys, DateTimeOffset FetchedAt);

    private static readonly ConcurrentDictionary<Uri, CacheEntry> JwksCache = new();
    private static Func<HttpClient>? JwksHttpClientFactory { get; set; }

     /// <summary>
     /// Removes a cached client JWKS so the next fetch refreshes it. Callers use this when a kid
     /// lookup misses (G5: refresh-on-miss) because the published key set may have rotated.
     /// </summary>
    internal static void InvalidateClientJwksCache(Uri jwksUri)
     => JwksCache.TryRemove(jwksUri, out _);

    internal static void ConfigureJwksHttpClientFactory(Func<HttpClient>? factory)
    {
        JwksHttpClientFactory = factory;
    }

    internal static void ResetJwksCache()
    {
        JwksCache.Clear();
    }

    public static async Task<TokenValidationParameters> CreateValidationParameters(
        this Client client,
        IJwksStore jwksStore,
        string? audience = null,
        string? issuer = null,
        bool forClientAuthentication = false,
        CancellationToken cancellationToken = default)
      {
        var signingKeys = await client
             .GetSigningCredentials(jwksStore, forClientAuthentication, cancellationToken)
             .ConfigureAwait(false);
        var encryptionKeys = client.JsonWebKeys == null ? [] : client.JsonWebKeys.GetEncryptionKeys().ToArray();
        if (encryptionKeys.Length == 0 && client.IdTokenEncryptedResponseAlg != null)
        {
            var key = await jwksStore.GetEncryptionKey(client.IdTokenEncryptedResponseAlg, cancellationToken)
                .ConfigureAwait(false);

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
            parameters.ValidAudiences = [audience, $"{audience.TrimEnd('/')}/token"];
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
            : await client.GenerateIdToken(jwsPayload, jwksStore, cancellationToken).ConfigureAwait(false);
    }

    extension(Client client)
    {
        private async Task<SigningCredentials?[]> GetSigningCredentials(
            IJwksStore jwksStore,
            bool forClientAuthentication = false,
            CancellationToken cancellationToken = default)
           {
            var jwks = client.JsonWebKeys;
            var hasEmbeddedKeys = jwks != null && jwks.Keys.Count > 0;
            if (!hasEmbeddedKeys && client.JwksUri != null)
             {
                jwks = await GetClientJwksAsync(client.JwksUri, cancellationToken).ConfigureAwait(false);
             }

            var signingKeyIds = jwks?.Keys
                  .Where(key => string.IsNullOrWhiteSpace(key.Use) || key.Use == JsonWebKeyUseNames.Sig)
                  .Select(key => key.Kid)
                  .ToArray()
                ?? [];

             // G5 refresh-on-miss: when a client authenticates via a jwks_uri and no matching
             // signature key was resolved, the published set may have rotated. Evict the cached
             // copy and fetch once more before giving up.
            if (forClientAuthentication && (jwks == null || jwks.Keys.Count == 0) && client.JwksUri != null)
             {
                InvalidateClientJwksCache(client.JwksUri);
                jwks = await GetClientJwksAsync(client.JwksUri, cancellationToken, refresh: true).ConfigureAwait(false);
                signingKeyIds = jwks?.Keys
                      .Where(key => string.IsNullOrWhiteSpace(key.Use) || key.Use == JsonWebKeyUseNames.Sig)
                      .Select(key => key.Kid)
                      .ToArray()
                   ?? [];
             }

             var signingKeys = jwks?.Keys
                  .Where(key => signingKeyIds.Contains(key.Kid))
                  .Select(key => new SigningCredentials(key, key.Alg))
                  .ToArray()
               ?? [];

            if (signingKeys?.Length != 0)
              {
                return signingKeys!;
               }

              // G6: a client must authenticate with its own key material. Never fall through to the
              // OP's default signing key when validating a client assertion — an empty set yields a
              // clean validation failure instead of silent misbehavior.
            if (forClientAuthentication)
              {
                return [];
               }

            var keys = await (client.IdTokenSignedResponseAlg == null
                      ? jwksStore.GetDefaultSigningKey(cancellationToken)
                      : jwksStore.GetSigningKey(client.IdTokenSignedResponseAlg, cancellationToken))
                  .ConfigureAwait(false);

            return [keys];
           }

        public async Task<string> GenerateIdToken(
            JwtPayload jwsPayload,
            IJwksStore jwksStore,
            CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var signingCredentials =
                await client.GetSigningCredentials(jwksStore, cancellationToken: cancellationToken).ConfigureAwait(false);
            var claimsIdentity = new ClaimsIdentity(jwsPayload.Claims);
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

        private static async Task<JsonWebKeySet?> GetClientJwksAsync(
            Uri jwksUri,
            CancellationToken cancellationToken,
            bool refresh = false)
          {
             // G5 SSRF guard: a client-supplied jwks_uri must not point at a private / loopback / link-local
                         // address, otherwise the AS could be coerced into probing internal hosts.
            if (IsPrivateAddress(jwksUri))
             {
                return null;
             }

            if (!refresh && JwksCache.TryGetValue(jwksUri, out var cached))
             {
                var notStale = DateTimeOffset.UtcNow - cached.FetchedAt < JwksCacheTtl;
                if (notStale)
                 {
                    return cached.Keys;
                 }

                 // TTL expired: fall through and refresh.
              }

            try
             {
               using var request = new HttpRequestMessage(HttpMethod.Get, jwksUri);
               request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               using var response = await GetJwksHttpClient().SendAsync(
                       request,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken)
                   .ConfigureAwait(false);
               response.EnsureSuccessStatusCode();
               var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
               var jwks = new JsonWebKeySet(json);
               JwksCache[jwksUri] = new CacheEntry(jwks, DateTimeOffset.UtcNow);
               return jwks;
             }
             catch (OperationCanceledException)
             {
                 throw;
             }
             catch (HttpRequestException)
             {
                 return null;
             }
             catch (ArgumentException)
             {
                 return null;
             }
             catch (SecurityTokenException)
             {
                 // A transient fetch failure should not permanently poison the cache; return null
                 // and let a later request (or refresh-on-miss) retry.
                return null;
             }
          }

          /// <summary>
          /// Whether a jwks_uri resolves to an address the AS must not fetch (loopback / private /
          /// link-local / unspecified). Covers the common loopback names plus the RFC 1918 private,
          /// 169.254 link-local, 127/8 loopback, and 100.64/10 carrier-grade NAT ranges, and the
          /// IPv6 equivalents.
          /// </summary>
        private static bool IsPrivateAddress(Uri uri)
          {
            if (uri.IsLoopback)
             {
                return true;
             }

            var host = uri.Host;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || host.Equals("[::1]", StringComparison.OrdinalIgnoreCase)
                    || host == "::1"
                    || host == "ip6-localhost")
             {
                return true;
             }

            if (!IPAddress.TryParse(host, out var ip))
             {
                 return false;
             }

            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
             {
                var octets = ip.GetAddressBytes();
                var first = octets[0];
                var second = octets[1];
                // 10/8, 172.16/12, 192.168/16, 127/8, 169.254/16, 100.64/10, 0.0.0.0/8
                return first is 10 or 127 or 172 or 192 or 169 or 0 or 100
                  || (first == 172 && second is >= 16 and <= 31)
                  || (first == 192 && second == 168)
                  || (first == 169 && second == 254)
                  || (first == 100 && second is >= 64 and <= 127)
                  || first == 0;
             }

             // IPv6: link-local (fe80::/10) and unique local (fc00::/7 = fc/fd).
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
             {
                var bytes = ip.GetAddressBytes();
                if (bytes[0] == 0xfe)
                 {
                    return (bytes[1] & 0xc0) == 0x80; // fe80/10
                 }

                 return (bytes[0] & 0xfe) == 0xfc; // fc00/7
             }

            return false;
          }

        private static HttpClient CreateJwksHttpClient()
        {
             var handler = new SocketsHttpHandler
             {
                 AllowAutoRedirect = false,
                 AutomaticDecompression = DecompressionMethods.None,
                 UseCookies = false
             };

             return new HttpClient(handler, disposeHandler: false)
             {
                 Timeout = TimeSpan.FromSeconds(5)
             };
        }

        private static HttpClient GetJwksHttpClient()
        {
             return JwksHttpClientFactory?.Invoke() ?? CreateJwksHttpClient();
        }
    }
}
