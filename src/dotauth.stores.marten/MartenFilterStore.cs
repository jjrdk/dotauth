namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Stores.Marten.Containers;
using global::Marten;

/// <summary>
/// Defines the Marten based filter repository.
/// </summary>
/// <seealso cref="IFilterStore" />
public sealed class MartenFilterStore : IFilterStore
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenFilterStore"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenFilterStore(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<Filter[]> GetAll(CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var filters = await session.Query<FilterContainer>().ToListAsync(token: cancellationToken)
            .ConfigureAwait(false);
        return filters.Select(x => x.ToFilter()).ToArray();
    }
}
