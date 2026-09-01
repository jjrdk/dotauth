namespace DotAuth.AuthServerPg;

using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Shared;
using DotAuth.Shared.Events.Logging;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the trace event publisher.
/// </summary>
public sealed partial class LogEventPublisher : IEventPublisher
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
        var json = JsonSerializer.Serialize(evt, SharedSerializerContext.Default.Options);
        if (typeof(DotAuthError).IsAssignableFrom(typeof(T)))
        {
            LogJson(LogLevel.Error, json);
        }
        else
        {
            LogJson(LogLevel.Information, json);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage("{Json}")]
    partial void LogJson(LogLevel logLevel, string json);
}
