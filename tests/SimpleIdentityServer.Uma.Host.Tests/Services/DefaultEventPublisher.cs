namespace SimpleIdentityServer.Uma.Host.Tests.Services
{
    using Shared;

    internal sealed class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
