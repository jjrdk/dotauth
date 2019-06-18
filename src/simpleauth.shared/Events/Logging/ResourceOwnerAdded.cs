namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    /// <summary>
    /// Defines the resource owner added event.
    /// </summary>
    /// <seealso cref="InfoMessage" />
    public class ResourceOwnerAdded : InfoMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnerAdded"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        public ResourceOwnerAdded(string id, string message, DateTime timestamp) : base(id, message, timestamp) { }
    }
}