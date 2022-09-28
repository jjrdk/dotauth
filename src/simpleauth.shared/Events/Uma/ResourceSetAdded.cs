namespace SimpleAuth.Shared.Events.Uma;

using System;

/// <summary>
/// Defines the resource set added event.
/// </summary>
public sealed record ResourceSetAdded : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceSetAdded"/> record.
    /// </summary>
    /// <param name="id">The id of the added resource set.</param>
    /// <param name="owner">The subject of the resource set owner.</param>
    /// <param name="name">The name of the resource set.</param>
    /// <param name="timestamp">The timestamp of the event.</param>
    public ResourceSetAdded(string id, string owner, string name, DateTimeOffset timestamp) : base(id, timestamp)
    {
        Owner = owner;
        Name = name;
    }

    /// <summary>
    /// Gets the subject of the resource set owner.
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// Gets the name of the resource set.
    /// </summary>
    public string Name { get; }
}