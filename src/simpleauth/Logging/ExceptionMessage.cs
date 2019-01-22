namespace SimpleAuth.Logging
{
    using System;
    using Shared;

    public class ExceptionMessage : Event
    {
        public ExceptionMessage(string id, Exception exception, DateTime timestamp)
            : base(id, timestamp)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}