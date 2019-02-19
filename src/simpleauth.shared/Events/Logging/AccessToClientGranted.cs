namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    public class AccessToClientGranted : Event
    {
        public AccessToClientGranted(
            string id,
            string clientId,
            string scopes,
            DateTime timestamp)
            : base(id, timestamp)
        {
            ClientId = clientId;
            Scopes = scopes;
        }

        public string ClientId { get; }
        public string Scopes { get; }
    }
}