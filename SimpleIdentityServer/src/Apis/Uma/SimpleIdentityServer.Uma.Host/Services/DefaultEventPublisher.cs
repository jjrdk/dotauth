using SimpleBus.Core;

namespace SimpleIdentityServer.Uma.Host.Services
{
    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
