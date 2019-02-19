namespace SimpleAuth.Shared.Events.Logging
{
    using System;

    public class ConsentGiven : InfoMessage
    {
        public ConsentGiven(string id, string message, DateTime timestamp) : base(id, message, timestamp)
        {
        }

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
