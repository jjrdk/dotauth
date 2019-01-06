namespace SimpleAuth.Shared.Events.Uma
{
    using System;

    public class AuthorizationPolicyNotAuthorized : Event
    {
        public string TicketId { get; }

        public AuthorizationPolicyNotAuthorized(string id, string ticketId, DateTime timestamp) : base(id, timestamp)
        {
            TicketId = ticketId;
        }
    }
}
