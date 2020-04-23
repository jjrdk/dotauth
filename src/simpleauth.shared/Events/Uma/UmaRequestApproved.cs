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

        /// <summary>
        /// Gets the approver subject.
        /// </summary>
        public string ApproverSubject { get; }

        /// <summary>
        /// Gets the requester's id claims (if any).
        /// </summary>
        public Claim[] RequesterClaims { get; }
    }
}