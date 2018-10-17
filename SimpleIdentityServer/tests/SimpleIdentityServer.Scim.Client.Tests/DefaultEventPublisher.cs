namespace SimpleIdentityServer.Scim.Client.Tests
{
    using Common.Dtos;
    using SimpleIdentityServer.Core.Common;

    public class DefaultEventPublisher : IEventPublisher
    {
        public void Publish<T>(T evt) where T : Event
        {
        }
    }
}