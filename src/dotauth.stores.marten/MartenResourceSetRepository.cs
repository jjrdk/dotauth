namespace DotAuth.Stores.Marten;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using global::Marten;
using global::Marten.Pagination;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the marten based resource set repository.
/// </summary>
/// <seealso cref="IResourceSetRepository" />
public sealed class MartenResourceSetRepository : IResourceSetRepository
{
    private readonly Func<IDocumentSession> _sessionFactory;
    private readonly ILogger<MartenResourceSetRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    /// <param name="logger">The logger</param>
    public MartenResourceSetRepository(
        Func<IDocumentSession> sessionFactory,
        ILogger<MartenResourceSetRepository> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ResourceSetDescription>> Search(
        IReadOnlyList<Claim> claims,
        SearchResourceSet parameter,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = _sessionFactory();
            await using var _ = session.ConfigureAwait(false);
            var queryable = session.Query<OwnedResourceSet>().Where(set =>
                set.Name.IsOneOf(parameter.Terms)
             || set.Description.NgramSearch(string.Join(' ', parameter.Terms))
             || set.Type.IsOneOf(parameter.Terms));
            if (parameter.Types.Length > 0)
            {
                queryable = queryable.Where(set => set.Type.IsOneOf(parameter.Types));
            }

            const string search = "search";
            var issuer = claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            var clientId = claims.GetClientId();
            queryable = queryable.Where(set => set.AuthorizationPolicies.Any(p =>
                (p.Scopes.AsEnumerable().Contains(search)
                 && p.ClientIdsAllowed.Length == 0
                 && p.OpenIdProvider == null)
             || (p.Scopes.AsEnumerable().Contains(search)
                 && p.ClientIdsAllowed.AsEnumerable().Contains(clientId)
                 && p.OpenIdProvider == null)
             || (p.Scopes.AsEnumerable().Contains(search)
                 && p.ClientIdsAllowed.Length == 0
                 && p.OpenIdProvider == issuer)
             || (p.Scopes.AsEnumerable().Contains(search)
                 && p.ClientIdsAllowed.AsEnumerable().Contains(clientId)
                 && p.OpenIdProvider == issuer)));

            var query = queryable
                .OrderBy(x => x.Name)
                .Select(x => new ResourceSetDescription
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Type = x.Type
                });

            var pageNumber = (parameter.StartIndex / parameter.PageSize) + 1;
            var resultSet = await query.ToPagedListAsync(
                pageNumber: pageNumber,
                pageSize: parameter.PageSize,
                token: cancellationToken);
            return cancellationToken.IsCancellationRequested
                ? new PagedResult<ResourceSetDescription>()
                : new PagedResult<ResourceSetDescription>
                {
                    Content = resultSet.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = resultSet.TotalItemCount
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}", ex.Message);
            return new PagedResult<ResourceSetDescription>
            {
                Content = [],
                StartIndex = 0,
                TotalResults = 0
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> Add(string owner, ResourceSet resourceSet, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Store(OwnedResourceSet.FromResourceSet(resourceSet, owner));
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var resourceSet = await session.LoadAsync<OwnedResourceSet>(id, cancellationToken).ConfigureAwait(false);

        return resourceSet?.Owner == owner
            ? resourceSet.AsResourceSet()
            : null;
    }

    /// <inheritdoc />
    public async Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var resourceSets = await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);
        var owners = resourceSets.Select(x => x.Owner).Distinct();

        return owners.SingleOrDefault();
    }

    /// <inheritdoc />
    public async Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var existing = await session.LoadAsync<OwnedResourceSet>(resourceSet.Id, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null)
        {
            return new ErrorDetails
            {
                Status = HttpStatusCode.NotFound,
                Title = ErrorCodes.NotUpdated,
                Detail = SharedStrings.ResourceCannotBeUpdated
            };
        }

        session.Update(OwnedResourceSet.FromResourceSet(resourceSet, existing.Owner));
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new Option.Success();
    }

    /// <inheritdoc />
    public async Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var resourceSets = await session.Query<OwnedResourceSet>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return resourceSets.Select(x => x.AsResourceSet()).ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> Remove(string id, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        session.Delete<OwnedResourceSet>(id);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var resourceSets =
            await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);

        return resourceSets.Select(x => x.AsResourceSet()).ToArray();
    }
}
