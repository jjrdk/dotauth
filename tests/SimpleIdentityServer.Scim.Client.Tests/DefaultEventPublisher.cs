namespace SimpleAuth.Scim.Client.Tests
{
    using Shared;

    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}