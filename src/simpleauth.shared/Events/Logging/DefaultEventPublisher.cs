namespace SimpleAuth.Shared.Events.Logging
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    internal class DefaultEventPublisher : IEventPublisher
    {
        public Task Publish<T>(T evt) where T : Event
        {
            return Task.CompletedTask;
        }
    }
}
