﻿namespace DotAuth.AuthServerPg;

using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Shared;
using DotAuth.Shared.Events.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Defines the trace event publisher.
/// </summary>
public sealed class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEventPublisher"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    public LogEventPublisher(ILogger<LogEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Publish<T>(T evt)
        where T : Event
    {
        var json = JsonConvert.SerializeObject(evt);
        if (typeof(DotAuthError).IsAssignableFrom(typeof(T)))
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
