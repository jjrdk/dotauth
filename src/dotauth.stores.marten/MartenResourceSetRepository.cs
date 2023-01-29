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
    public MartenResourceSetRepository(Func<IDocumentSession> sessionFactory, ILogger<MartenResourceSetRepository> logger)
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
            await using var session = _sessionFactory();
            var command = CreateQueryCommand(parameter, claims, session);
            var reader = await session.ExecuteReaderAsync(command, cancellationToken);
            var resultSet = new List<ResourceSetDescription>();
            var totalCount = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var set = await session.DocumentStore.Advanced.Serializer.FromJsonAsync<ResourceSetDescription>(
                    reader,
                    0,
                    cancellationToken);

                resultSet.Add(set);

                totalCount = Math.Max(totalCount, reader.GetInt32(1));
            }

            return cancellationToken.IsCancellationRequested
                ? new PagedResult<ResourceSetDescription>()
                : new PagedResult<ResourceSetDescription>
                {
                    Content = resultSet.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = totalCount
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{error}", ex.Message);
            return new PagedResult<ResourceSetDescription>
            {
                Content = Array.Empty<ResourceSetDescription>(),
                StartIndex = 0,
                TotalResults = 0
            };
        }
    }

    private static NpgsqlCommand CreateQueryCommand(SearchResourceSet parameter, IReadOnlyList<Claim> claims, IQuerySession session)
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

        builder.Append(" AND (d.data @> @onlySearchScope OR d.data @> @onlyClient OR d.data @> @onlyProvider OR d.data @> @clientAndProvider)");
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

        if (parameter.Terms.Length>0)
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
                    new
                    {
                        clients = Array.Empty<string>(),
                        scopes = new[] { "search" },
                        provider = (string?)null
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
                    new
                    {
                        clients = new[] { clientId },
                        scopes = new[] { "search" },
                        provider = (string?)null
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
                    new { clients = Array.Empty<string>(), scopes = new[] { "search" }, provider = provider }
                }
            },
            NpgsqlDbType.Jsonb);
        command.AddNamedParameter(
            "clientAndProvider",
            new
            {
                authorization_policies = new[]
                {
                    new
                    {
                        clients = new[] { clientId },
                        scopes = new[] { "search" },
                        provider = provider
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
        await using var session = _sessionFactory();
        session.Store(OwnedResourceSet.FromResourceSet(resourceSet, owner));
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        var resourceSet = await session.LoadAsync<OwnedResourceSet>(id, cancellationToken).ConfigureAwait(false);

        return resourceSet?.Owner == owner
            ? resourceSet.AsResourceSet()
            : null;
    }

    /// <inheritdoc />
    public async Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids)
    {
        await using var session = _sessionFactory();
        var resourceSets = await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);
        var owners = resourceSets.Select(x => x.Owner).Distinct();

        return owners.SingleOrDefault();
    }

    /// <inheritdoc />
    public async Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        var existing = await session.LoadAsync<OwnedResourceSet>(resourceSet.Id, cancellationToken).ConfigureAwait(false);
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
        await using var session = _sessionFactory();
        var resourceSets = await session.Query<OwnedResourceSet>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return resourceSets.Select(x => x.AsResourceSet()).ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> Remove(string id, CancellationToken cancellationToken)
    {
        await using var session = _sessionFactory();
        session.Delete<OwnedResourceSet>(id);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids)
    {
        await using var session = _sessionFactory();
        var resourceSets =
            await session.LoadManyAsync<OwnedResourceSet>(cancellationToken, ids).ConfigureAwait(false);

        return resourceSets.Select(x => x.AsResourceSet()).ToArray();
    }
}