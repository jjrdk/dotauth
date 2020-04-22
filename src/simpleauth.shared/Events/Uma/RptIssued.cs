namespace SimpleAuth.Shared.Events.Uma
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// Defines the requesting party token issued event.
    /// </summary>
    public class RptIssued : UmaTicketEvent
    {
        /// <inheritdoc />
        public RptIssued(string id, string ticketId, string clientId, string resourceOwner, IEnumerable<Claim> requester, DateTimeOffset timestamp)
            : base(id, ticketId, clientId, requester, timestamp)
        {
            ResourceOwner = resourceOwner;
        }

        /// <summary>
        /// Gets the resource owner for the requested resource.
        /// </summary>
        public string ResourceOwner { get; }
    }
}