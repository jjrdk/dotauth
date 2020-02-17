namespace SimpleAuth.AuthServerPgRedis
{
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Events.Logging;

    /// <summary>
    /// Defines the trace event publisher.
    /// </summary>
    public class LogEventPublisher : IEventPublisher
    {
        private readonly ILogger<LogEventPublisher> _logger;

        public LogEventPublisher(ILogger<LogEventPublisher> logger)
        {
            _logger = logger;
        }

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
                _logger.LogError(json);
            }
            else
            {
                _logger.LogInformation(json);
            }
            return Task.CompletedTask;
        }
    }
}