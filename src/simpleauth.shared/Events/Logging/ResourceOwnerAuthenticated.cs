namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    /// <summary>
    /// Defines the resource owner authenticated event.
    /// </summary>
    /// <seealso cref="Event" />
    public record ResourceOwnerAuthenticated : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnerAdded"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="subject">The account subject</param>
        /// <param name="timestamp">The timestamp.</param>
        public ResourceOwnerAuthenticated(string id, string subject, DateTimeOffset timestamp)
            : base(id, timestamp)
        {
            Subject = subject;
        }

        /// <summary>
        /// Gets the subject of the removed resource owner.
        /// </summary>
        public string Subject { get; }
    }
}