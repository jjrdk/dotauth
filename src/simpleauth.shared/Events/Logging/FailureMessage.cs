namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the failure message event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class FailureMessage : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailureMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        public FailureMessage(string id, string message, DateTime timestamp)
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