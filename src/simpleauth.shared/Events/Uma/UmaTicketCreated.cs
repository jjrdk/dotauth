namespace SimpleAuth.Shared.Events.Uma
{
    using System;
    using System.Security.Claims;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the UMA ticket created event.
    /// </summary>
    public class UmaTicketCreated : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaTicketCreated"/> class.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <param name="clientId">The id of the requesting client.</param>
        /// <param name="ticketId">The ticket id.</param>
        /// <param name="requests">The permission requests.</param>
        /// <param name="requesterClaims">The claim identifying the requester.</param>
        /// <param name="timestamp">The timestamp of the event.</param>
        public UmaTicketCreated(
            string id,
            string clientId,
            string ticketId,
            Claim[] requesterClaims,
            DateTimeOffset timestamp,
            params PermissionRequest[] requests)
            : base(id, timestamp)
        {
            ClientId = clientId;
            TicketId = ticketId;
            RequesterClaims = requesterClaims;
            Requests = requests;
        }

        /// <summary>
        /// Gets the id of the requesting client.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the id of the created ticket.
        /// </summary>
        public string TicketId { get; }

        /// <summary>
        /// Gets the claims identifying the requester.
        /// </summary>
        public Claim[] RequesterClaims { get; }

        /// <summary>
        /// Gets the permission request.
        /// </summary>
        public PermissionRequest[] Requests { get; }
    }
}
