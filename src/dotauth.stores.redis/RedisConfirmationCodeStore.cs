namespace DotAuth.Stores.Redis;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using StackExchange.Redis;

public sealed class RedisConfirmationCodeStore : IConfirmationCodeStore
{
    private readonly IDatabaseAsync _database;
    private readonly TimeSpan _expiry;

    public RedisConfirmationCodeStore(IDatabaseAsync database, TimeSpan expiry = default)
    {
        _database = database;
        _expiry = expiry == default ? TimeSpan.FromMinutes(30) : expiry;
    }

    public async Task<ConfirmationCode?> Get(string code, string subject, CancellationToken cancellationToken)
    {
        var confirmationCode = await _database.StringGetAsync(code).ConfigureAwait(false);
        return confirmationCode.HasValue
            ? JsonSerializer.Deserialize<ConfirmationCode>(confirmationCode!,
                SharedSerializerContext.Default.ConfirmationCode)
            : null;
    }

    public Task<bool> Add(ConfirmationCode confirmationCode, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(confirmationCode, SharedSerializerContext.Default.ConfirmationCode);
        return _database.StringSetAsync(confirmationCode.Value, json, _expiry, when: When.NotExists);
    }

    public Task<bool> Remove(string code, string subject, CancellationToken cancellationToken)
    {
        return _database.KeyDeleteAsync(code);
    }
}
