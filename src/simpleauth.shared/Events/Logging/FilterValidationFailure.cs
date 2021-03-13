namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the failure message event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public record FilterValidationFailure : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterValidationFailure"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        public FilterValidationFailure(string id, string message, DateTimeOffset timestamp)
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