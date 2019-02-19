namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the exception message event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class ExceptionMessage : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="timestamp">The timestamp.</param>
        public ExceptionMessage(string id, Exception exception, DateTime timestamp)
            : base(id, timestamp)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; }
    }
}