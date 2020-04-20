namespace SimpleAuth.Shared.Events.Uma
{
    using System;

    /// <summary>
    /// Defines the UMA request approved event.
    /// </summary>
    /// <seealso cref="UmaTicketEvent" />
    public class UmaRequestApproved : UmaTicketEvent
    {
        /// <inheritdoc />
        public UmaRequestApproved(string id, string ticketid, string clientId, string approverSubject, DateTimeOffset timestamp)
            : base(id, ticketid, clientId, null, timestamp)
        {
            ApproverSubject = approverSubject;
        }

        public string ApproverSubject { get; }
    }
}