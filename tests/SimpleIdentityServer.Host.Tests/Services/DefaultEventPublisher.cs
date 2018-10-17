namespace SimpleIdentityServer.Host.Tests.Services
{
    using Common.Dtos;
    using Core.Common;

    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}
