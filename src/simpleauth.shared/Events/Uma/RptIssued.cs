namespace SimpleAuth.Shared.Events.Uma;

using System;
using System.Collections.Generic;
using SimpleAuth.Shared.Models;

/// <summary>
/// Defines the requesting party token issued event.
/// </summary>
public sealed record RptIssued : UmaTicketEvent
{
    /// <inheritdoc />
    public RptIssued(string id, string ticketId, string clientId, string resourceOwner, IEnumerable<ClaimData> requester, DateTimeOffset timestamp)
        : base(id, ticketId, clientId, requester, timestamp)
    {
        ResourceOwner = resourceOwner;
    }

    /// <summary>
    /// Gets the resource owner for the requested resource.
    /// </summary>
    public string ResourceOwner { get; }
}