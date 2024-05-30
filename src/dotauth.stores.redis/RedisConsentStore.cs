namespace DotAuth.Stores.Redis;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using StackExchange.Redis;

public sealed class RedisConsentStore : IConsentRepository
{
    private readonly IDatabaseAsync _database;
    private readonly TimeSpan _expiry;

    public RedisConsentStore(IDatabaseAsync database, TimeSpan expiry = default)
    {
        _database = database;
        _expiry = expiry == default ? TimeSpan.FromDays(365 * 5) : expiry;
    }

    public async Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
        string subject,
        CancellationToken cancellationToken)
    {
        var consent = await _database.StringGetAsync(subject).ConfigureAwait(false);
        return consent.HasValue
            ? JsonSerializer.Deserialize<Consent[]>(consent!, DefaultJsonSerializerOptions.Instance)!
            : [];
    }

    public Task<bool> Insert(Consent record, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(record, DefaultJsonSerializerOptions.Instance);
        return _database.StringSetAsync(record.Subject, json, _expiry);
    }

    public Task<bool> Delete(Consent record, CancellationToken cancellationToken)
    {
        return _database.KeyDeleteAsync(record.Subject);
    }
}
