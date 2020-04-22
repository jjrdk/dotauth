namespace SimpleAuth.Shared.Events.Uma
{
    using System;
    using System.Security.Claims;

    /// <summary>
    /// Defines the UMA request approved event.
    /// </summary>
    /// <seealso cref="UmaTicketEvent" />
    public class UmaRequestApproved : UmaTicketEvent
    {
        /// <inheritdoc />
        public UmaRequestApproved(string id, string ticketid, string clientId, string approverSubject, Claim[] requesterClaims, DateTimeOffset timestamp)
            : base(id, ticketid, clientId, null, timestamp)
        {
            ApproverSubject = approverSubject;
            RequesterClaims = requesterClaims;
        }

        public string ApproverSubject { get; }

        public Claim[] RequesterClaims { get; }
    }
}