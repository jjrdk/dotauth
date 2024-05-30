namespace DotAuth.Shared.Events.Uma;

using System;
using System.Collections.Generic;
using System.Linq;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the UMA request not authorized event.
/// </summary>
/// <seealso cref="Event" />
public abstract record UmaTicketEvent : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaRequestNotAuthorized"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ticketId">The ticket.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="requester">The ticket requester.</param>
    /// <param name="timestamp">The timestamp.</param>
    protected UmaTicketEvent(
        string id,
        string ticketId,
        string clientId,
        IEnumerable<ClaimData>? requester,
        DateTimeOffset timestamp)
        : base(id, timestamp)
    {
        TicketId = ticketId;
        ClientId = clientId;
        Requester = requester == null
            ? []
            : requester.ToArray();
    }

    /// <summary>
    /// Gets the ticket.
    /// </summary>
    /// <value>
    /// The ticket.
    /// </value>
    public string TicketId { get; }

    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string ClientId { get; }

    /// <summary>
    /// Gets the ticket requester.
    /// </summary>
    public ClaimData[] Requester { get; }
}