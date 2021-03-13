namespace SimpleAuth.Shared.Events.Uma
{
    using System;
    using System.Security.Claims;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the UMA ticket created event.
    /// </summary>
    public record UmaTicketCreated : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaTicketCreated"/> class.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <param name="clientId">The id of the requesting client.</param>
        /// <param name="ticketId">The ticket id.</param>
        /// <param name="requests">The permission requests.</param>
        /// <param name="requester">The subject of the requester.</param>
        /// <param name="requesterClaims">The claim identifying the requester.</param>
        /// <param name="timestamp">The timestamp of the event.</param>
        /// <param name="resourceOwner">The subject of the resource owner.</param>
        public UmaTicketCreated(
            string id,
            string clientId,
            string ticketId,
            string resourceOwner,
            string requester,
            ClaimData[] requesterClaims,
            DateTimeOffset timestamp,
            params PermissionRequest[] requests)
            : base(id, timestamp)
        {
            ClientId = clientId;
            TicketId = ticketId;
            ResourceOwner = resourceOwner;
            Requester = requester;
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
        /// Gets the subject of the resource owner.
        /// </summary>
        public string ResourceOwner { get; }

        /// <summary>
        /// Gets the subject of the requester.
        /// </summary>
        public string Requester { get; }

        /// <summary>
        /// Gets the claims identifying the requester.
        /// </summary>
        public ClaimData[] RequesterClaims { get; }

        /// <summary>
        /// Gets the permission request.
        /// </summary>
        public PermissionRequest[] Requests { get; }
    }
}
