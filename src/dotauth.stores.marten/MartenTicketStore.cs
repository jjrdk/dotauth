namespace DotAuth.Stores.Marten;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using global::Marten;

/// <summary>
/// Defines the marten based ticket store.
/// </summary>
/// <seealso cref="ITicketStore" />
public sealed class MartenTicketStore : ITicketStore
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenTicketStore(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Store(ticket);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<(bool success, ClaimData[] requester)> ApproveAccess(
        string ticketId,
        CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var ticket = await session.LoadAsync<Ticket>(ticketId, cancellationToken).ConfigureAwait(false);
        if (ticket == null)
        {
            return (false, Array.Empty<ClaimData>());
        }

        if (ticket.IsAuthorizedByRo)
        {
            return (true, ticket.Requester);
        }

        ticket = ticket with { IsAuthorizedByRo = true };
        session.Store(ticket);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (true, ticket.Requester);
    }

    /// <inheritdoc />
    public async Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Delete<Ticket>(ticketId);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<Ticket?> Get(string ticketId, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var ticket = await session.LoadAsync<Ticket>(ticketId, cancellationToken).ConfigureAwait(false);

        return ticket;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;
        var tickets = await session.Query<Ticket>()
            .Where(x => x.ResourceOwner == owner && x.Created <= now && x.Expires > now)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return tickets.ToArray();
    }

    /// <inheritdoc />
    public async Task Clean(CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.DeleteWhere<Ticket>(t => t.Expires <= DateTimeOffset.UtcNow);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}