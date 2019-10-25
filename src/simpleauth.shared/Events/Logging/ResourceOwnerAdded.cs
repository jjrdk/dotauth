namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using DTOs;

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
        /// <param name="claims">The resource owner claims.</param>
        /// <param name="timestamp">The timestamp.</param>
        public ResourceOwnerAdded(string id, string subject, PostClaim[] claims, DateTimeOffset timestamp) : base(id, timestamp)
        {
            Subject = subject;
            Claims = claims;
        }

        /// <summary>
        /// Gets the subject of the added resource owner.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the claims for the added resource owner.
        /// </summary>
        public PostClaim[] Claims { get; }
    }
}