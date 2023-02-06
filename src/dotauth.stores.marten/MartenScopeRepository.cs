namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using global::Marten;
using global::Marten.Pagination;

/// <summary>
/// Defines the Marten based scope repository.
/// </summary>
/// <seealso cref="IScopeRepository" />
public sealed class MartenScopeRepository : IScopeRepository
{
    private readonly Func<IDocumentSession> _sessionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    public MartenScopeRepository(Func<IDocumentSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    /// <inheritdoc />
    public async Task<PagedResult<Scope>> Search(SearchScopesRequest parameter, CancellationToken cancellationToken = default)
    {
        var session = this._sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var results = await session.Query<Scope>()
            .Where(x => x.Name.IsOneOf(parameter.ScopeNames) && x.Type.IsOneOf(parameter.ScopeTypes))
            .ToPagedListAsync(parameter.StartIndex + 1, parameter.NbResults, cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Scope>
        {
            Content = results.ToArray(),
            StartIndex = parameter.StartIndex,
            TotalResults = results.TotalItemCount
        };
    }

    /// <inheritdoc />
    public async Task<Scope?> Get(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var scope = await session.LoadAsync<Scope>(name, cancellationToken).ConfigureAwait(false);

        return scope;
    }

    /// <inheritdoc />
    public async Task<Scope[]> SearchByNames(CancellationToken cancellationToken = default, params string[] names)
    {
        var session = this._sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var scopes = await session.Query<Scope>()
            .Where(x => x.Name.IsOneOf(names))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return scopes.ToArray();
    }

    /// <inheritdoc />
    public async Task<Scope[]> GetAll(CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var scopes = await session.Query<Scope>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return scopes.ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> Insert(Scope scope, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Store(scope);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> Delete(Scope scope, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Delete(scope.Name);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> Update(Scope scope, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Update(scope);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}