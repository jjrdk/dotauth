namespace DotAuth.Stores.Marten.Containers;

using System;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the storage container type for <see cref="Filter"/>.
/// </summary>
public sealed record FilterContainer : Filter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterContainer"/> class.
    /// </summary>
    /// <param name="id">The id of the container.</param>
    /// <param name="name">The name of the filter.</param>
    /// <param name="rules">The filter rules.</param>
    public FilterContainer(string id, string name, params FilterRule[] rules) : base(name, rules)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public string Id { get; } = null!;

    /// <summary>
    /// Converts the contents of this container to a <see cref="Filter"/> instance.
    /// </summary>
    /// <returns>A <see cref="Filter"/> instance.</returns>
    public Filter ToFilter()
    {
        return new Filter(Name, Rules);
    }

    /// <summary>
    /// Creates a new <see cref="FilterContainer"/> from the specified <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">The <paramref name="filter"/> to copy content from.</param>
    /// <returns>A <see cref="FilterContainer"/> instance.</returns>
    public static FilterContainer Create(Filter filter)
    {
        return new FilterContainer(Guid.NewGuid().ToString("N"), filter.Name, filter.Rules);
    }
}
