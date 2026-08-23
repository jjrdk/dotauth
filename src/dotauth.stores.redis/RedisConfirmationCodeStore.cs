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
/// Redis backed implementation of the <see cref="IConfirmationCodeStore"/>.
/// </summary>
public sealed class RedisConfirmationCodeStore : IConfirmationCodeStore
{
    private readonly IDatabaseAsync _database;
    private readonly ITenantContext _tenantContext;
    private readonly TimeSpan _expiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConfirmationCodeStore"/> class.
    /// </summary>
    /// <param name="database">The Redis database</param>
    /// <param name="tenantContext">The <see cref="ITenantContext"/>.</param>
    /// <param name="expiry">The cache expiry. Defaults to 30 minutes.</param>
    public RedisConfirmationCodeStore(IDatabaseAsync database, ITenantContext tenantContext, TimeSpan expiry = default)
    {
        _database = database;
        _tenantContext = tenantContext;
        _expiry = expiry == TimeSpan.Zero ? TimeSpan.FromMinutes(30) : expiry;
    }

    /// <summary>Returns a key namespaced to the current tenant to prevent cross-tenant access.</summary>
    private string Key(string value) => $"{_tenantContext.TenantId}:{value}";

    /// <inheritdoc/>
    public async Task<ConfirmationCode?> Get(string code, string subject, CancellationToken cancellationToken)
    {
        var confirmationCode = await _database.StringGetAsync(Key(code)).ConfigureAwait(false);
        return confirmationCode.HasValue
            ? JsonSerializer.Deserialize<ConfirmationCode>(confirmationCode.ToString(),
                SharedSerializerContext.Default.ConfirmationCode)
            : null;
    }

    /// <inheritdoc/>
    public Task<bool> Add(ConfirmationCode confirmationCode, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(confirmationCode, SharedSerializerContext.Default.ConfirmationCode);
        return _database.StringSetAsync(Key(confirmationCode.Value), json, _expiry, when: When.NotExists);
    }

    /// <inheritdoc/>
    public Task<bool> Remove(string code, string subject, CancellationToken cancellationToken)
    {
        return _database.KeyDeleteAsync(Key(code));
    }
}
