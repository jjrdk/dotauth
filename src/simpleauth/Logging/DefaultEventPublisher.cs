namespace SimpleAuth.Logging
{
    using System.Threading.Tasks;
    using Shared;

    internal class DefaultEventPublisher : IEventPublisher
    {
        public Task Publish<T>(T evt) where T : Event
        {
            return Task.CompletedTask;
        }
    }
}
