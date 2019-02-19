namespace SimpleAuth.Shared.Events.Uma
{
    using System;

    public class UmaRequestNotAuthorized : Event
    {
        public UmaRequestNotAuthorized(string id, string ticket, string clientId, DateTime timestamp)
            : base(id, timestamp)
        {
            Ticket = ticket;
            ClientId = clientId;
        }

        public string Ticket { get; }
        public string ClientId { get; }
    }
}
