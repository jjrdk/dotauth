namespace DotAuth.Stores.Marten;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
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
using JasperFx.Core;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Weasel.Postgresql;

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
                (p.Scopes.Contains(search)
                 && p.ClientIdsAllowed.Length == 0
                 && p.OpenIdProvider == null)
             || (p.Scopes.Contains(search)
                 && p.ClientIdsAllowed.Contains(clientId)
                 && p.OpenIdProvider == null)
             || (p.Scopes.Contains(search)
                 && p.ClientIdsAllowed.Length == 0
                 && p.OpenIdProvider == issuer)
             || (p.Scopes.Contains(search)
                 && p.ClientIdsAllowed.Contains(clientId)
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

    private static NpgsqlCommand CreateQueryCommand(
        SearchResourceSet parameter,
        IReadOnlyList<Claim> claims,
        IQuerySession session)
    {
        var builder = new StringBuilder();
        builder.Append(
            "SELECT d.data, count(d.data) over (range unbounded preceding) total from mt_doc_ownedresourceset d WHERE ");
        if (parameter.Terms.Length > 0)
        {
            builder.Append(
                "(d.name = any (@terms) OR string_to_array(d.description, ' ') && (@terms) OR d.type = any (@terms))");
        }

        if (parameter.Types.Length > 0)
        {
            builder.Append(" AND d.type = any (@type)");
        }

        builder.Append(
            " AND (d.data @> @onlySearchScope OR d.data @> @onlyClient OR d.data @> @onlyProvider OR d.data @> @clientAndProvider)");
        builder.Append(
            @" AND exists 
 (
    select policies from 
    (
			select jsonb_array_elements(d.data -> 'authorization_policies') policies
			from mt_doc_ownedresourceset d
    ) p
    where @claims @> (p.policies -> 'claims') 
 )");

        builder.Append(" ORDER BY (d.data ->> 'name')");
        if (parameter.PageSize > 0)
        {
            builder.Append(" LIMIT @limit");
        }

        if (parameter.StartIndex > 0)
        {
            builder.Append(" OFFSET @offset");
        }

        //var serializer = session.DocumentStore.Advanced.Serializer;
        var provider = claims.First(c => c.Type == "iss").Value;
        var command = new NpgsqlCommand(builder.ToString(), session.Connection);

        if (parameter.Terms.Length > 0)
        {
            command.AddNamedParameter("terms", parameter.Terms, NpgsqlDbType.Array | NpgsqlDbType.Text);
        }

        if (parameter.Types.Length > 0)
        {
            command.AddNamedParameter("type", parameter.Types, NpgsqlDbType.Array | NpgsqlDbType.Varchar);
        }

        var clientId = claims.GetClientId();
        command.AddNamedParameter(
            "onlySearchScope",
            new
            {
                authorization_policies = new[]
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = [],
                        Scopes = ["search"],
                        OpenIdProvider = null
                    }
                }
            },
            NpgsqlDbType.Jsonb);
        command.AddNamedParameter(
            "onlyClient",
            new
            {
                authorization_policies = new[]
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = [clientId],
                        Scopes = ["search"],
                        OpenIdProvider = null
                    }
                }
            },
            NpgsqlDbType.Jsonb);
        command.AddNamedParameter(
            "onlyProvider",
            new
            {
                authorization_policies = new[]
                {
                    new PolicyRule { ClientIdsAllowed = [], Scopes = ["search"], OpenIdProvider = provider }
                }
            },
            NpgsqlDbType.Jsonb);
        command.AddNamedParameter(
            "clientAndProvider",
            new
            {
                authorization_policies = new[]
                {
                    new PolicyRule
                    {
                        ClientIdsAllowed = [clientId],
                        Scopes = ["search"],
                        OpenIdProvider = provider
                    }
                }
            },
            NpgsqlDbType.Jsonb);
        command.AddNamedParameter("claims", claims.Select(ClaimData.FromClaim).ToArray(), NpgsqlDbType.Jsonb);
        if (parameter.PageSize > 0)
        {
            command.AddNamedParameter("limit", parameter.PageSize, NpgsqlDbType.Integer);
        }

        if (parameter.StartIndex > 0)
        {
            command.AddNamedParameter("offset", parameter.StartIndex * parameter.PageSize, NpgsqlDbType.Integer);
        }

        return command;
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
