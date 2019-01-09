namespace SimpleAuth.Shared.Events.Openid
{
    using System;
    using Models;

    public class ExternalUserCreated : Event
    {
        public ExternalUserCreated(string id, ResourceOwner resourceOwner, DateTime timestamp) : base(id, timestamp)
        {
            ResourceOwner = resourceOwner;
        }

        public ResourceOwner ResourceOwner { get; }
    }
}