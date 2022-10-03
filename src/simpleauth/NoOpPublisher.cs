namespace DotAuth;

using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Shared;

internal sealed class NoOpPublisher : IEventPublisher
{
    public Task Publish<T>(T evt) where T : Event
    {
        return Task.CompletedTask;
    }
}