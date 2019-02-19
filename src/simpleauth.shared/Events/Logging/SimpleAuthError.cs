namespace SimpleAuth.Shared.Events.Logging
{
    using System;
    using SimpleAuth.Shared;

    public class SimpleAuthError : Event
    {
        public SimpleAuthError(string id, string code, string description, string state, DateTime timestamp) : base(id, timestamp)
        {
            Code = code;
            Description = description;
            State = state;
        }

        public string Code { get; }
        public string Description { get; }
        public string State { get; }
    }
}