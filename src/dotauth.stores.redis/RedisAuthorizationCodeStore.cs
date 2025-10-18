namespace DotAuth.Stores.Redis;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using StackExchange.Redis;

/// <summary>
/// Defines the Redis authorization code store.
/// </summary>
public sealed class RedisAuthorizationCodeStore : IAuthorizationCodeStore
{
    private readonly IDatabaseAsync _database;
    private readonly TimeSpan _expiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisAuthorizationCodeStore"/> class.
    /// </summary>
    /// <param name="database">The underlying Redis store.</param>
    /// <param name="expiry">The default cache expiration.</param>
    public RedisAuthorizationCodeStore(IDatabaseAsync database, TimeSpan expiry = default)
    {
        _database = database;
        _expiry = expiry == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : expiry;
    }

    /// <inheritdoc />
    public async Task<AuthorizationCode?> Get(string code, CancellationToken cancellationToken)
    {
        var authCode = await _database.StringGetAsync(code).ConfigureAwait(false);
        return authCode.HasValue
            ? JsonSerializer.Deserialize<AuthorizationCode>(authCode!, SharedSerializerContext.Default.AuthorizationCode)
            : null;
    }

    /// <inheritdoc />
    public Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(authorizationCode, SharedSerializerContext.Default.AuthorizationCode);
        return _database.StringSetAsync(authorizationCode.Code, json, _expiry);
    }

    /// <inheritdoc />
    public Task<bool> Remove(string code, CancellationToken cancellationToken)
    {
        return _database.KeyDeleteAsync(code);
    }
}
