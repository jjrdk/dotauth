namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    /// <summary>
    /// Defines the authorization code granted message.
    /// </summary>
    /// <seealso cref="InfoMessage" />
    public class AuthorizationCodeGranted : InfoMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCodeGranted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        public AuthorizationCodeGranted(string id, string message, DateTime timestamp)
            : base(id, message, timestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCodeGranted"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <param name="scopes">The scopes.</param>
        public AuthorizationCodeGranted(string clientId, string authorizationCode, string scopes)
            : this(
                Shared.Id.Create(),
                $"Grant authorization code to the client {clientId}, authorization code : {authorizationCode} and scopes : {scopes}",
                DateTime.UtcNow)
        {
        }
    }
}
