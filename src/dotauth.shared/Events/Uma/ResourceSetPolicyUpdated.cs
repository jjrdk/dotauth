namespace DotAuth.Shared.Events.Uma;

using System;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the resource set policy updated event.
/// </summary>
public sealed record ResourceSetPolicyUpdated : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceSetPolicyUpdated"/> record.
    /// </summary>
    /// <param name="id">The id of the source</param>
    /// <param name="oldPolicy">The policy before update.</param>
    /// <param name="newPolicy">The policy after update.</param>
    /// <param name="timestamp">The timestamp of the event.</param>
    public ResourceSetPolicyUpdated(string id, PolicyRule[] oldPolicy, PolicyRule[] newPolicy, DateTimeOffset timestamp) : base(id, timestamp)
    {
        OldPolicy = oldPolicy;
        NewPolicy = newPolicy;
    }

    /// <summary>
    /// Gets the policy before the update.
    /// </summary>
    public PolicyRule[] OldPolicy { get; }

    /// <summary>
    /// Gets the policy after the update.
    /// </summary>
    public PolicyRule[] NewPolicy { get; }
}