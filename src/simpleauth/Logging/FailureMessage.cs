namespace SimpleAuth.Logging
{
    using Shared;
    using System;

    public class FailureMessage : Event
    {
        public FailureMessage(string id, string message, DateTime timestamp)
        : base(id, timestamp)
        {
            Message = message;
        }

        public string Message { get; }
    }
}