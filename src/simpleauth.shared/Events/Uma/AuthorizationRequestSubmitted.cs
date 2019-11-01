namespace SimpleAuth.Shared.Events.Uma
{
    using System;

    /// <summary>
    /// Defines the authorization request submitted event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class AuthorizationRequestSubmitted : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRequestSubmitted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="timestamp">The timestamp.</param>
        public AuthorizationRequestSubmitted(string id, string ticketId, DateTimeOffset timestamp)
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
}