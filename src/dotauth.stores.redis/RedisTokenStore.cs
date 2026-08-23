namespace DotAuth.Stores.Redis;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using StackExchange.Redis;

/// <summary>
/// The Redis backed implementation of the <see cref="ITokenStore"/> interface.
/// </summary>
public sealed class RedisTokenStore : ITokenStore
{
    private readonly IDatabaseAsync _database;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisTokenStore"/> class.
    /// </summary>
    /// <param name="database">The Redis database</param>
    /// <param name="tenantContext">The <see cref="ITenantContext"/></param>
    public RedisTokenStore(IDatabaseAsync database, ITenantContext tenantContext)
    {
        _database = database;
        _tenantContext = tenantContext;
    }

    /// <summary>Returns a key namespaced to the current tenant to prevent cross-tenant access.</summary>
    private string Key(string value) => $"{_tenantContext.TenantId}:{value}";

    /// <inheritdoc/>
    public async Task<GrantedToken?> GetToken(
        string scopes,
        string clientId,
        JwtPayload? idTokenJwsPayload,
        JwtPayload? userInfoJwsPayload,
        CancellationToken cancellationToken = default)
    {
        var token = await _database.StringGetAsync(Key(clientId + scopes)).ConfigureAwait(false);
        var options = token.HasValue
            ? JsonSerializer.Deserialize<GrantedToken[]>(token.ToString(), SharedSerializerContext.Default.GrantedTokenArray)!
            : [];
        return options.FirstOrDefault(
            x =>
            {
                var hasSameIdToken = (idTokenJwsPayload == null && x.IdTokenPayLoad == null)
                 || idTokenJwsPayload?.All(a => x.IdTokenPayLoad?.Contains(a) == true) == true;
                var hasSameUserInfoToken = (userInfoJwsPayload == null && x.UserInfoPayLoad == null)
                 || userInfoJwsPayload?.All(a => x.UserInfoPayLoad?.Contains(a) == true)
                 == true;
                return hasSameIdToken && hasSameUserInfoToken;
            });
    }

    /// <inheritdoc/>
    public Task<GrantedToken?> GetRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        return GetSingleToken(refreshToken, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        return GetSingleToken(accessToken, cancellationToken);
    }

    private async Task<GrantedToken?> GetSingleToken(string token, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(Key(token)).ConfigureAwait(false);
        return value.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<GrantedToken>(value.ToString(), SharedSerializerContext.Default.GrantedToken);
    }

    /// <inheritdoc/>
    public async Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
    {
        var value = JsonSerializer.Serialize(grantedToken, SharedSerializerContext.Default.GrantedToken);
        var scopeKey = Key(grantedToken.ClientId + grantedToken.Scope);
        var existingScopeValue = await _database.StringGetAsync(scopeKey).ConfigureAwait(false);
        var existingScopeToken = existingScopeValue.HasValue
            ? JsonSerializer.Deserialize<GrantedToken[]>(existingScopeValue.ToString(), SharedSerializerContext.Default.GrantedTokenArray)!
            : [];
        var scopeTokens = JsonSerializer.Serialize(existingScopeToken.Concat([grantedToken]).ToArray(),
            SharedSerializerContext.Default.GrantedTokenArray);
        var expiry = TimeSpan.FromSeconds(grantedToken.ExpiresIn);
        var idTask = _database.StringSetAsync(Key(grantedToken.Id), value, expiry, when: When.NotExists);
        var scopeTokenTask = _database.StringSetAsync(scopeKey, scopeTokens, expiry, when: When.NotExists);
        var accessTokenTask = _database.StringSetAsync(Key(grantedToken.AccessToken), value, expiry, when: When.NotExists);
        var refreshTokenTask = grantedToken.RefreshToken == null
            ? Task.FromResult(true)
            : _database.StringSetAsync(Key(grantedToken.RefreshToken), value, expiry, when: When.NotExists);

        var result = (await Task.WhenAll(idTask, scopeTokenTask, accessTokenTask, refreshTokenTask)
                .ConfigureAwait(false))
            .All(x => x);
        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await GetRefreshToken(refreshToken, cancellationToken).ConfigureAwait(false);
        return token != null && await RemoveToken(token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        var token = await GetSingleToken(accessToken, cancellationToken).ConfigureAwait(false);
        return token != null && await RemoveToken(token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<GrantedToken?> ConsumeRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        // Use a small Lua script to atomically GET and DEL the refresh token key.
        var key = Key(refreshToken);
        const string script = "local v = redis.call('GET', KEYS[1]); if not v then return nil; end; redis.call('DEL', KEYS[1]); return v;";
        try
        {
            var result = await _database.ScriptEvaluateAsync(script, [key]).ConfigureAwait(false);
            if (result.IsNull)
            {
                return null;
            }

            var value = (string)result!;
            var grantedToken = JsonSerializer.Deserialize<GrantedToken>(value, SharedSerializerContext.Default.GrantedToken);

            // Attempt to remove the other associated keys (id, scope, access token). We don't fail
            // the consume operation if cleanup doesn't fully succeed here; best-effort cleanup is OK.
            if (grantedToken != null)
            {
                // Fire-and-forget cleanup, but await to keep ordering predictable.
                try
                {
                    await RemoveToken(grantedToken).ConfigureAwait(false);
                }
                catch
                {
                    // Ignore cleanup errors - consume has succeeded.
                }
            }

            return grantedToken;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> RemoveToken(GrantedToken grantedToken)
    {
        var idTask = _database.KeyDeleteAsync(Key(grantedToken.Id));
        var scopeTokenTask = _database.KeyDeleteAsync(Key(grantedToken.ClientId + grantedToken.Scope));
        var accessTokenTask = _database.KeyDeleteAsync(Key(grantedToken.AccessToken));
        var refreshTokenTask = grantedToken.RefreshToken == null
            ? Task.FromResult(true)
            : _database.KeyDeleteAsync(Key(grantedToken.RefreshToken));

        try
        {
            var result = (await Task.WhenAll(idTask, scopeTokenTask, accessTokenTask, refreshTokenTask)
                    .ConfigureAwait(false))
                .All(x => x);
            return result;
        }
        catch
        {
            return false;
        }
    }
}
