namespace SimpleAuth.Logging
{
    using System;

    public class AuthorizationCodeGranted : InfoMessage
    {
        public AuthorizationCodeGranted(string id, string message, DateTime timestamp) : base(id, message, timestamp)
        {
        }

        public AuthorizationCodeGranted(
            string clientId,
            string authorizationCode,
            string scopes)
            : this(Shared.Id.Create(),
                $"Grant authorization code to the client {clientId}, authorization code : {authorizationCode} and scopes : {scopes}",
                DateTime.UtcNow)
        {
        }
    }
}