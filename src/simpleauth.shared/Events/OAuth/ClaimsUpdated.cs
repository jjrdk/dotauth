namespace SimpleAuth.Shared.Events.OAuth
{
    using System;
    using DTOs;

    /// <summary>
    /// Defines the claims updated event.
    /// </summary>
    public class ClaimsUpdated : Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsUpdated"/> class.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <param name="subject">The subject of the updated claims.</param>
        /// <param name="from">Claims list before update.</param>
        /// <param name="to">Claims list after update.</param>
        /// <param name="timestamp">The time stamp of the event.</param>
        public ClaimsUpdated(string id, string subject, PostClaim[] from, PostClaim[] to, DateTimeOffset timestamp)
            : base(id, timestamp)
        {
            Subject = subject;
            From = @from;
            To = to;
        }

        /// <summary>
        /// Gets the subject of the updated claims.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the claims before update.
        /// </summary>
        public PostClaim[] From { get; }

        /// <summary>
        /// Gets the claims after update.
        /// </summary>
        public PostClaim[] To { get; }
    }
}