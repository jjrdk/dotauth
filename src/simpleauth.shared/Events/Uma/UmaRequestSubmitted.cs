namespace SimpleAuth.Shared.Events.Uma
{
    using System;
    using System.Security.Claims;

    /// <summary>
    /// Defines the UMA request not authorized event.
    /// </summary>
    /// <seealso cref="Event" />
    public class UmaRequestSubmitted : UmaTicketEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaRequestNotAuthorized"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ticket">The ticket.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="timestamp">The timestamp.</param>
        public UmaRequestSubmitted(string id, string ticket, string clientId, ClaimsPrincipal requester, DateTimeOffset timestamp)
            : base(id, ticket, clientId, requester, timestamp)
        {
        }
    }
}