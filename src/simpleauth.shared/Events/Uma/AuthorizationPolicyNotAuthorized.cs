namespace DotAuth.Shared.Events.Uma;

using System;

/// <summary>
/// Defines the authorization policy not authorized event.
/// </summary>
/// <seealso cref="Event" />
public sealed record AuthorizationPolicyNotAuthorized : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationPolicyNotAuthorized"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ticketId">The ticket identifier.</param>
    /// <param name="timestamp">The timestamp.</param>
    public AuthorizationPolicyNotAuthorized(string id, string ticketId, DateTimeOffset timestamp)
        : base(id, timestamp)
    {
        TicketId = ticketId;
    }

    /// <summary>
    /// Gets the ticket identifier.
    /// </summary>
    /// <value>
    /// The ticket identifier.
    /// </value>
    public string TicketId { get; }
}