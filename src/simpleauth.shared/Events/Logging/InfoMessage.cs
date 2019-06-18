namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the info message event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public abstract class InfoMessage : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        protected InfoMessage(string id, string message, DateTime timestamp)
        : base(id, timestamp)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }
    }
}