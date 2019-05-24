namespace SimpleAuth.Shared.Events
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
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

            var json = JsonConvert.SerializeObject(evt);
            if (typeof(SimpleAuthError).IsAssignableFrom(typeof(T)))
            {
                Trace.TraceError(json);
            }
            else
            {
                Trace.TraceInformation(json);
            }
            return Task.CompletedTask;
        }
    }
}