namespace DotAuth.Uma;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the token cache class.
/// </summary>
public abstract class ClientTokenCache : ITokenCache
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly List<StoredToken> _tokens;
    private readonly TokenClient _tokenClient;
    private readonly IUmaPermissionClient _permissionClient;
    private (DateTimeOffset, JsonWebKeySet?)? _jsonWebKeySet;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientTokenCache"/> class.
    /// </summary>
    /// <param name="clientId">The id of the client application.</param>
    /// <param name="clientSecret">The secret of the client application.</param>
    /// <param name="authority">The <see cref="Uri"/> of the discovery document.</param>
    /// <param name="backChannelHandler">The request handler.</param>
    protected ClientTokenCache(
        string clientId,
        string clientSecret,
        Uri authority,
        HttpMessageHandler? backChannelHandler = null)
    {
        HttpClient ClientFunc() => new(backChannelHandler ?? new HttpClientHandler { AllowAutoRedirect = true });
        _tokens = new List<StoredToken>();
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(clientId, clientSecret),
            ClientFunc,
            authority);
        _permissionClient = new UmaClient(ClientFunc, authority);
    }

    protected abstract AuthenticateClientBase CreateAuthenticateClient(string[] scopes);

    /// <inheritdoc />
    public async ValueTask<GrantedTokenResponse?> GetUmaToken(
        string idToken,
        CancellationToken cancellationToken = default,
        params PermissionRequest[] permissions)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            var storedToken = _tokens.OfType<StoredRptToken>()
                .FirstOrDefault(t => t.Permissions.IsSupersetOf(permissions));
            if (storedToken != null)
            {
                if (DateTime.UtcNow < storedToken.Expiration)
                {
                    return storedToken.GrantedToken;
                }

                _tokens.Remove(storedToken);
            }

            var ticketResponse = await _permissionClient.RequestPermission(
                    idToken,
                    cancellationToken,
                    permissions.Select(
                            p => new PermissionRequest
                            {
                                IdToken = idToken,
                                ResourceSetId = p.ResourceSetId,
                                Scopes = p.Scopes
                            })
                        .ToArray())
                .ConfigureAwait(false);
            if (ticketResponse is not Option<TicketResponse>.Result result)
            {
                return null;
            }

            var rptTokenResponse = await _tokenClient.GetToken(
                    TokenRequest.FromTicketId(result.Item.TicketId, idToken),
                    cancellationToken)
                .ConfigureAwait(false);

            if (rptTokenResponse is not Option<GrantedTokenResponse>.Result tokenResult)
            {
                return null;
            }

            _tokens.Add(
                new StoredRptToken(
                    tokenResult.Item,
                    DateTimeOffset.UtcNow.AddSeconds(tokenResult.Item.ExpiresIn),
                    permissions));

            return tokenResult.Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_jsonWebKeySet?.Item2 == null || _jsonWebKeySet.Value.Item1.AddHours(12) < DateTimeOffset.UtcNow)
            {
                _jsonWebKeySet = (DateTimeOffset.UtcNow,
                                  await _tokenClient.GetJwks(cancellationToken).ConfigureAwait(false));
            }

            return _jsonWebKeySet.Value.Item2;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask<GrantedTokenResponse?> GetToken(
        CancellationToken cancellationToken = default,
        params string[] scopes)
    {
        if (HasAccessToken(scopes))
        {
            return _tokens.OfType<StoredAccessToken>().First(x => x.Scopes.IsSubsetOf(scopes)).GrantedToken;
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            var client = CreateAuthenticateClient(scopes);
            var tokenResponse = await client.LogIn(cancellationToken).ConfigureAwait(false);
            if (tokenResponse is not Option<GrantedTokenResponse>.Result result)
            {
                return null;
            }

            UpdateCachedTokens(result.Item);
            return result.Item;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void UpdateCachedTokens(GrantedTokenResponse grantedTokenResponse)
    {
        var scopes = grantedTokenResponse.Scope.Split(' ');
        var toRemove = _tokens.OfType<StoredAccessToken>().Where(x => x.Scopes.IsSubsetOf(scopes)).ToArray();
        foreach (var storedAccessToken in toRemove)
        {
            _tokens.Remove(storedAccessToken);
        }

        _tokens.Add(
            new StoredAccessToken(
                grantedTokenResponse,
                DateTimeOffset.UtcNow.AddSeconds(grantedTokenResponse.ExpiresIn),
                scopes));
    }

    /// <inheritdoc />
    public bool HasAccessToken(params string[] scopes)
    {
        return _tokens.OfType<StoredAccessToken>().Any(x => x.Scopes.IsSupersetOf(scopes));
    }

    private abstract class StoredToken
    {
        protected StoredToken(GrantedTokenResponse tokenResponse, DateTimeOffset expiration)
        {
            Expiration = expiration;
            GrantedToken = tokenResponse;
        }

        public DateTimeOffset Expiration { get; }

        public GrantedTokenResponse GrantedToken { get; }
    }

    private class StoredAccessToken : StoredToken
    {
        /// <inheritdoc />
        public StoredAccessToken(GrantedTokenResponse tokenResponse, DateTimeOffset expiration, string[] scopes)
            : base(tokenResponse, expiration)
        {
            Scopes = new HashSet<string>(scopes);
        }

        public HashSet<string> Scopes { get; }
    }

    private class StoredRptToken : StoredToken
    {
        /// <inheritdoc />
        public StoredRptToken(
            GrantedTokenResponse tokenResponse,
            DateTimeOffset expiration,
            params PermissionRequest[] permissions)
            : base(tokenResponse, expiration)
        {
            Permissions = new HashSet<PermissionRequest>(permissions);
        }

        public HashSet<PermissionRequest> Permissions { get; }
    }
}
