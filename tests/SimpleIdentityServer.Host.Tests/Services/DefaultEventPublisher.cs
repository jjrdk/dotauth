namespace SimpleIdentityServer.Host.Tests.Services
{
    using SimpleAuth.Shared;

    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
