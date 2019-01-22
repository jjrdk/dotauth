namespace SimpleAuth.Logging
{
    using Shared;
    using System;

    public class AccessToClientGranted : Event
    {
        public AccessToClientGranted(
            string id,
            string clientId,
            string accessToken,
            string scopes,
            DateTime timestamp)
            : base(id, timestamp)
        {
            ClientId = clientId;
            AccessToken = accessToken;
            Scopes = scopes;
        }

        public string ClientId { get; }
        public string AccessToken { get; }
        public string Scopes { get; }
    }
}