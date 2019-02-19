namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the access to client granted event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class AccessToClientGranted : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessToClientGranted"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="scopes">The scopes.</param>
        /// <param name="timestamp">The timestamp.</param>
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

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; }

        /// <summary>
        /// Gets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        public string Scopes { get; }
    }
}