namespace SimpleAuth.Logging
{
    using Shared;
    using System;

    public abstract class InfoMessage : Event
    {
        protected InfoMessage(string id, string message, DateTime timestamp)
        : base(id, timestamp)
        {
            Message = message;
        }

        public string Message { get; }
    }
}