namespace SimpleAuth.Shared.Events
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Logging;

    /// <summary>
    /// Defines the trace event publisher.
    /// </summary>
    public class TraceEventPublisher : IEventPublisher
    {
        /// <inheritdoc />
        public Task Publish<T>(T evt)
            where T : Event
        {
            if (evt == null)
            {
                return Task.CompletedTask;
            }
            if (typeof(SimpleAuthError).IsAssignableFrom(typeof(T)))
            {
                Trace.TraceError(evt.ToString());
            }
            else
            {
                Trace.TraceInformation(evt.ToString());
            }
            return Task.CompletedTask;
        }
    }
}