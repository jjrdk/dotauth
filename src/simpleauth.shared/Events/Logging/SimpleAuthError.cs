namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the error event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Event" />
    public class SimpleAuthError : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthError"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="state">The state.</param>
        /// <param name="timestamp">The timestamp.</param>
        public SimpleAuthError(string id, string code, string description, string state, DateTime timestamp) : base(id, timestamp)
        {
            Code = code;
            Description = description;
            State = state;
        }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; }
    }
}