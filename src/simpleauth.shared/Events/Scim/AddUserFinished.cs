namespace SimpleAuth.Shared.Events.Scim
{
    using System;

    public class AddUserFinished : Event
    {
        public AddUserFinished(string id, string processId, string payload, DateTime timestamp)
            : base(id, timestamp)
        {
            ProcessId = processId;
            Payload = payload;
        }

        public string ProcessId { get; }
        public string Payload { get; }
    }
}
