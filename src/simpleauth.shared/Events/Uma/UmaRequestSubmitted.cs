namespace SimpleAuth.Shared.Events.Uma;

using System;
using System.Collections.Generic;
using SimpleAuth.Shared.Models;

/// <summary>
/// Defines the UMA request not authorized event.
/// </summary>
/// <seealso cref="Event" />
public sealed record UmaRequestSubmitted : UmaTicketEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaRequestNotAuthorized"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ticket">The ticket.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="requester">The ticket requester claims.</param>
    /// <param name="timestamp">The timestamp.</param>
    public UmaRequestSubmitted(string id, string ticket, string clientId, IEnumerable<ClaimData> requester, DateTimeOffset timestamp)
        : base(id, ticket, clientId, requester, timestamp)
    {
    }
}