namespace SimpleAuth.Shared.Events.OAuth
{
    using System;
    using DTOs;

    public class ClaimsUpdated : Event
    {
        public ClaimsUpdated(string id, string subject, PostClaim[] from, PostClaim[] to, DateTime timestamp)
            : base(id, timestamp)
        {
            Subject = subject;
            From = @from;
            To = to;
        }

        public string Subject { get; }

        public PostClaim[] From { get; }

        public PostClaim[] To { get; }
    }
}