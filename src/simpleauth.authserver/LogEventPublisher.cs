﻿namespace SimpleAuth.AuthServer
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SimpleAuth.Events;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Events.Logging;

    /// <summary>
    /// Defines the trace event publisher.
    /// </summary>
    internal class LogEventPublisher : IEventPublisher
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
            var json = JsonConvert.SerializeObject(evt);
            if (evt is SimpleAuthError)
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