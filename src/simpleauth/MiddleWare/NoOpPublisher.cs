namespace SimpleAuth.Server.MiddleWare
{
    using System.Threading.Tasks;
    using Shared;

    public class NoOpPublisher : IEventPublisher
    {
        public Task Publish<T>(T evt) where T : Event
        {
            return Task.CompletedTask;
        }
    }
}