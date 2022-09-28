namespace SimpleAuth;

using System.Threading.Tasks;
using SimpleAuth.Events;
using SimpleAuth.Shared;

internal sealed class NoOpPublisher : IEventPublisher
{
    public Task Publish<T>(T evt) where T : Event
    {
        return Task.CompletedTask;
    }
}