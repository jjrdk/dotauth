namespace DotAuth.AuthServerPgRedis;

using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Shared;
using DotAuth.Shared.Events.Logging;
using Microsoft.Extensions.Logging;


/// <summary>
/// Defines the trace event publisher.
/// </summary>
public sealed class LogEventPublisher : IEventPublisher
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
        var json = JsonSerializer.Serialize(evt, DefaultJsonSerializerOptions.Instance);
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
