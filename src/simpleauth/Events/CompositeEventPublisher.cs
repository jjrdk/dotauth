namespace SimpleAuth.Events;

using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Shared;

/// <summary>
/// Defines the composite event publisher.
/// </summary>
public sealed class CompositeEventPublisher : IEventPublisher
{
    private readonly IEventPublisher[] _publishers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeEventPublisher"/> class.
    /// </summary>
    public CompositeEventPublisher(params IEventPublisher[] publishers)
    {
        _publishers = publishers.ToArray();
    }

    /// <inheritdoc />
    public Task Publish<T>(T evt)
        where T : Event
    {
        return Task.WhenAll(_publishers.Select(_ => _.Publish(evt)).ToArray());
    }
}