namespace SimpleAuth.Stores.Redis;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;
using StackExchange.Redis;

public sealed class RedisTokenStore : ITokenStore
{
    private readonly IDatabaseAsync _database;

    public RedisTokenStore(IDatabaseAsync database)
    {
        _database = database;
    }

    public async Task<GrantedToken?> GetToken(
        string scopes,
        string clientId,
        JwtPayload? idTokenJwsPayload,
        JwtPayload? userInfoJwsPayload,
        CancellationToken cancellationToken = default)
    {
        var token = await _database.StringGetAsync(clientId + scopes).ConfigureAwait(false);
        var options = token.HasValue
            ? JsonConvert.DeserializeObject<GrantedToken[]>(token!)!
            : Array.Empty<GrantedToken>();
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

    public async Task<GrantedToken?> GetRefreshToken(string getRefreshToken, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(getRefreshToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GrantedToken>(value!);
    }

    public async Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(accessToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GrantedToken>(value!);
    }

    public async Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken)
    {
        var value = JsonConvert.SerializeObject(grantedToken);
        var existingScopeValue = await _database.StringGetAsync(grantedToken.ClientId + grantedToken.Scope)
            .ConfigureAwait(false);
        var existingScopeToken = existingScopeValue.HasValue
            ? JsonConvert.DeserializeObject<GrantedToken[]>(existingScopeValue!)!
            : Array.Empty<GrantedToken>();
        var scopeTokens = JsonConvert.SerializeObject(existingScopeToken.Concat(new[] { grantedToken }).ToArray());
        var expiry = TimeSpan.FromSeconds(grantedToken.ExpiresIn);
        var idTask = _database.StringSetAsync(grantedToken.Id, value, expiry, when: When.NotExists);
        var scopeTokenTask = _database.StringSetAsync(
            grantedToken.ClientId + grantedToken.Scope,
            scopeTokens,
            expiry,
            when: When.NotExists);
        var accessTokenTask = _database.StringSetAsync(grantedToken.AccessToken, value, expiry, when: When.NotExists);

        var refreshTokenTask = grantedToken.RefreshToken == null
            ? Task.FromResult(true)
            : _database.StringSetAsync(grantedToken.RefreshToken, value, expiry, when: When.NotExists);

        if ((await Task.WhenAll(idTask, scopeTokenTask, accessTokenTask, refreshTokenTask).ConfigureAwait(false))
            .All(x => x))
        {
            return true;
        }

        return await RemoveToken(grantedToken).ConfigureAwait(false);
    }

    public async Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await GetRefreshToken(refreshToken, cancellationToken).ConfigureAwait(false);
        return token != null && await RemoveToken(token).ConfigureAwait(false);
    }

    public async Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken)
    {
        var token = await GetRefreshToken(accessToken, cancellationToken).ConfigureAwait(false);
        return token != null && await RemoveToken(token).ConfigureAwait(false);
    }

    private async Task<bool> RemoveToken(GrantedToken grantedToken)
    {
        var idTask = _database.KeyDeleteAsync(grantedToken.Id);
        var scopeTokenTask = _database.KeyDeleteAsync(grantedToken.ClientId + grantedToken.Scope);
        var accessTokenTask = _database.KeyDeleteAsync(grantedToken.AccessToken);
        var refreshTokenTask = grantedToken.RefreshToken == null
            ? Task.FromResult(true)
            : _database.KeyDeleteAsync(grantedToken.RefreshToken);

        return (await Task.WhenAll(idTask, scopeTokenTask, accessTokenTask, refreshTokenTask).ConfigureAwait(false))
            .All(x => x);
    }
}