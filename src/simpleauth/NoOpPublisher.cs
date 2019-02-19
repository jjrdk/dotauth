namespace SimpleAuth
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    public class NoOpPublisher : IEventPublisher
    {
        public Task Publish<T>(T evt) where T : Event
        {
            return Task.CompletedTask;
        }
    }
}