namespace DotAuth.Stores.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
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
        _expiry = expiry == TimeSpan.Zero ? TimeSpan.FromDays(365 * 5) : expiry;
    }

    public async Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
        string subject,
        CancellationToken cancellationToken)
    {
        return await GetStoredConsents(subject).ConfigureAwait(false);
    }

    public async Task<bool> Insert(Consent record, CancellationToken cancellationToken)
    {
        var consents = (await GetStoredConsents(record.Subject).ConfigureAwait(false)).ToList();
        var existingIndex = consents.FindIndex(c => c.Id == record.Id);
        if (existingIndex >= 0)
        {
            consents[existingIndex] = record;
        }
        else
        {
            consents.Add(record);
        }

        return await PersistConsents(record.Subject, consents).ConfigureAwait(false);
    }

    public async Task<bool> Delete(Consent record, CancellationToken cancellationToken)
    {
        var consents = (await GetStoredConsents(record.Subject).ConfigureAwait(false)).ToList();
        var removed = consents.RemoveAll(c => c.Id == record.Id) > 0;
        if (!removed)
        {
            return false;
        }

        return consents.Count == 0
            ? await _database.KeyDeleteAsync(record.Subject).ConfigureAwait(false)
            : await PersistConsents(record.Subject, consents).ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<Consent>> GetStoredConsents(string subject)
    {
        var storedConsents = await _database.StringGetAsync(subject).ConfigureAwait(false);
        if (!storedConsents.HasValue)
        {
            return [];
        }

        var json = storedConsents.ToString();
        try
        {
            var consents = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.ConsentArray);
            if (consents != null)
            {
                return consents;
            }
        }
        catch (JsonException)
        {
            // Backward compatibility with the previous single-consent payload shape.
        }

        var consent = JsonSerializer.Deserialize(json, SharedSerializerContext.Default.Consent);
        return consent == null ? [] : [consent];
    }

    private Task<bool> PersistConsents(string subject, IReadOnlyCollection<Consent> consents)
    {
        var json = JsonSerializer.Serialize(consents.ToArray(), SharedSerializerContext.Default.ConsentArray);
        return _database.StringSetAsync(subject, json, _expiry);
    }
}
