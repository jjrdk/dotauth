namespace SimpleAuth.Logging
{
    using System;

    public class ResourceOwnerAdded : InfoMessage
    {
        public ResourceOwnerAdded(string id, string message, DateTime timestamp) : base(id, message, timestamp) { }
    }
}