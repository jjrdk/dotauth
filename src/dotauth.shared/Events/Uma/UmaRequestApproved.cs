﻿namespace DotAuth.Shared.Events.Uma;

using System;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the UMA request approved event.
/// </summary>
/// <seealso cref="UmaTicketEvent" />
public sealed record UmaRequestApproved : UmaTicketEvent
{
    /// <inheritdoc />
    public UmaRequestApproved(string id, string ticketId, string clientId, string approverSubject, ClaimData[] requesterClaims, DateTimeOffset timestamp)
        : base(id, ticketId, clientId, null, timestamp)
    {
        ApproverSubject = approverSubject;
        RequesterClaims = requesterClaims;
    }

    /// <summary>
    /// Gets the approver subject.
    /// </summary>
    public string ApproverSubject { get; }

    /// <summary>
    /// Gets the requester's id claims (if any).
    /// </summary>
    public ClaimData[] RequesterClaims { get; }
}