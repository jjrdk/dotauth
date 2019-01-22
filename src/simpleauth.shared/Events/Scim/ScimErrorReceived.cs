namespace SimpleAuth.Shared.Events.Scim
{
    using System;

    public class ScimErrorReceived : Event
    {
        public ScimErrorReceived(string id, string processId, string message, DateTime timestamp)
            : base(id, timestamp)
        {
            ProcessId = processId;
            Message = message;
        }

        public string ProcessId { get; }
        public string Message { get; }
    }
}
