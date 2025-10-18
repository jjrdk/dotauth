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

/// <summary>
/// Defines the Redis ticket store.
/// </summary>
public sealed class RedisTicketStore : ITicketStore
{
    private readonly IDatabaseAsync _database;
    private readonly TimeSpan _expiry;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisTicketStore"/> class.
    /// </summary>
    /// <param name="database">The underlying Redis store.</param>
    /// <param name="expiry">The default cache expiration.</param>
    public RedisTicketStore(IDatabaseAsync database, TimeSpan expiry = default)
    {
        _database = database;
        _expiry = expiry == TimeSpan.Zero ? TimeSpan.FromDays(30) : expiry;
    }

    /// <inheritdoc />
    public Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(ticket, SharedSerializerContext.Default.Ticket);
        return _database.StringSetAsync(ticket.Id, json, _expiry);
    }

    /// <inheritdoc />
    public async Task<(bool success, ClaimData[] requester)> ApproveAccess(
        string ticketId,
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(ticketId).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return (false, []);
        }

        var ticket = JsonSerializer.Deserialize<Ticket>(value!, SharedSerializerContext.Default.Ticket)! with
        {
            IsAuthorizedByRo = true
        };
        var result = await _database.StringSetAsync(ticket.Id,
                JsonSerializer.Serialize(ticket, SharedSerializerContext.Default.Ticket), _expiry)
            .ConfigureAwait(false);

        return (result, result ? ticket.Requester : []);
    }

    /// <inheritdoc />
    public Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
    {
        return _database.KeyDeleteAsync(ticketId);
    }

    /// <inheritdoc />
    public async Task<Ticket?> Get(string ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _database.StringGetAsync(ticketId).ConfigureAwait(false);
        return ticket.HasValue
            ? JsonSerializer.Deserialize<Ticket>(ticket!, SharedSerializerContext.Default.Ticket)
            : null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public Task Clean(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
