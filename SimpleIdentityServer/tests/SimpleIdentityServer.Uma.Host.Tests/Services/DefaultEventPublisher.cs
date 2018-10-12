namespace SimpleIdentityServer.Uma.Host.Tests.Services
{
    using SimpleIdentityServer.Common.Dtos;
    using SimpleIdentityServer.Core.Common;

    internal sealed class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
