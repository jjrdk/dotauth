namespace DotAuth.Events;

using System.Threading.Tasks;
using DotAuth.Shared;

/// <summary>
/// Defines the default event publisher.
/// </summary>
public sealed class NoopEventPublisher : IEventPublisher
{
    /// <inheritdoc />
    public Task Publish<T>(T evt) where T : Event
    {
        return Task.CompletedTask;
    }
}