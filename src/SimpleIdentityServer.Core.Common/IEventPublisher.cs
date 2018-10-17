namespace SimpleIdentityServer.Core.Common
{
    using SimpleIdentityServer.Common.Dtos;

    public interface IEventPublisher
    {
        void Publish<T>(T evt) where T : Event;
    }
}
