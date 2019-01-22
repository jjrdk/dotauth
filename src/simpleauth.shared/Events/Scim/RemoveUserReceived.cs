namespace SimpleAuth.Shared.Events.Scim
{
    using System;

    public class RemoveUserReceived : Event
    {
        public RemoveUserReceived(string id, string processId, string payload, DateTime timestamp)
            : base(id, timestamp)
        {
            ProcessId = processId;
            Payload = payload;
        }

        public string ProcessId { get; }
        public string Payload { get; }
    }
}
