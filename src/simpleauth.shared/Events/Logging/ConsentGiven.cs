namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    /// <summary>
    /// Defines the consent given event.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.Events.Logging.InfoMessage" />
    public class ConsentGiven : InfoMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentGiven"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="timestamp">The timestamp.</param>
        public ConsentGiven(string id, string message, DateTime timestamp) : base(id, message, timestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentGiven"/> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="consentId">The consent identifier.</param>
        public ConsentGiven(
            string subject,
            string clientId,
            string consentId)
            : this(
                Shared.Id.Create(),
                $"The consent has been given by the resource owner, subject : {subject}, client id : {clientId}, consent id : {consentId}",
                DateTime.UtcNow)
        {
        }
    }
}
