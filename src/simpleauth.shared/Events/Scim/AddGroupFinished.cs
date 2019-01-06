namespace SimpleAuth.Shared.Events.Scim
{
    using System;

    public class AddGroupFinished : Event
    {
        public AddGroupFinished(string id, string processId, string payload, DateTime timestamp)
            : base(id, timestamp)
        {
            ProcessId = processId;
            Payload = payload;
        }

        public string ProcessId { get; }
        public string Payload { get; }
    }
}
