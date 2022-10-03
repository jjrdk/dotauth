namespace DotAuth.Shared.Repositories;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the in-memory filter store.
/// </summary>
public sealed class InMemoryFilterStore : IFilterStore
{
    private readonly Filter[] _filters;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryFilterStore"/> class.
    /// </summary>
    /// <param name="filters"></param>
    public InMemoryFilterStore(params Filter[] filters)
    {
        _filters = filters ?? Array.Empty<Filter>();
    }

    /// <inheritdoc />
    public Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_filters.ToArray());
    }
}