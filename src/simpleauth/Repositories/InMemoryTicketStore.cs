namespace SimpleAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared.Models;
using SimpleAuth.Shared.Repositories;

/// <summary>
/// Defines the in-memory ticket store.
/// </summary>
/// <seealso cref="ITicketStore" />
internal sealed class InMemoryTicketStore : ITicketStore
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<string, Ticket> _tickets;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTicketStore"/> class.
    /// </summary>
    public InMemoryTicketStore()
    {
        _tickets = new Dictionary<string, Ticket>
        {
            {
                "1234",
                new Ticket
                {
                    ResourceOwner = "administrator",
                    Created = DateTimeOffset.UtcNow.AddHours(-1),
                    Expires = DateTimeOffset.MaxValue,
                    Id = "1234",
                    IsAuthorizedByRo = false,
                    Lines = new[]
                    {
                        new TicketLine {ResourceSetId = "abc", Scopes = new[] {"read", "write"}},
                        new TicketLine {ResourceSetId = "def", Scopes = new[] {"read", "write", "print"}},
                    }
                }
            }
        };
    }

    /// <inheritdoc />
    public async Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            _tickets.Add(ticket.Id, ticket);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<(bool success, ClaimData[] requester)> ApproveAccess(
        string ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!_tickets.ContainsKey(ticketId))
            {
                return (false, Array.Empty<ClaimData>());
            }

            _tickets[ticketId] = _tickets[ticketId] with { IsAuthorizedByRo = true };
            return (true, _tickets[ticketId].Requester.ToArray());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Ticket?> Get(string ticketId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            throw new ArgumentNullException(nameof(ticketId));
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            _tickets.TryGetValue(ticketId, out var ticket);
            return ticket;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            throw new ArgumentNullException(nameof(owner));
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow;
            bool Predicate(Ticket x) => x.ResourceOwner == owner && x.Created <= now && x.Expires > now;
            return _tickets.Values.Where(Predicate).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            throw new ArgumentNullException(nameof(ticketId));
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return _tickets.Remove(ticketId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task Clean(CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var toRemove = _tickets
                .Where(x => x.Value.Expires <= DateTimeOffset.UtcNow)
                .Select(x => x.Key);
            foreach (var id in toRemove)
            {
                _tickets.Remove(id);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}