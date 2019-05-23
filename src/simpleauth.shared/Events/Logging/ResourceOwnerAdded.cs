namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    /// <summary>
    /// Defines the resource owner added event.
    /// </summary>
    /// <seealso cref="Event" />
    public class ResourceOwnerAdded : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOwnerAdded"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="subject">The resource owner subject.</param>
        /// <param name="timestamp">The timestamp.</param>
        public ResourceOwnerAdded(string id, string subject, DateTime timestamp) : base(id, timestamp)
        {
            Subject = subject;
        }

        public string Subject { get; }
    }
}