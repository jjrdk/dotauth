namespace SimpleAuth.Events
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the default event publisher.
    /// </summary>
    public class NoopEventPublisher : IEventPublisher
    {
        /// <inheritdoc />
        public Task Publish<T>(T evt) where T : Event
        {
            return Task.CompletedTask;
        }
    }
}
