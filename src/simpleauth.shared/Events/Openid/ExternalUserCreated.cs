namespace DotAuth.Shared.Events.Openid;

using System;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the external user created event.
/// </summary>
/// <seealso cref="Event" />
public sealed record ExternalUserCreated : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalUserCreated"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="resourceOwner">The resource owner.</param>
    /// <param name="timestamp">The timestamp.</param>
    public ExternalUserCreated(string id, ResourceOwner resourceOwner, DateTimeOffset timestamp) : base(id, timestamp)
    {
        ResourceOwner = resourceOwner;
    }

    /// <summary>
    /// Gets the resource owner.
    /// </summary>
    /// <value>
    /// The resource owner.
    /// </value>
    public ResourceOwner ResourceOwner { get; }
}