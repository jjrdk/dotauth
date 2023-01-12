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
    public async Task<PagedResult<ResourceSet>> Search(
        ClaimsPrincipal requestor,
        SearchResourceSet parameter,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var session = _sessionFactory();
            var command = CreateQueryCommand(parameter, requestor, session);
            var reader = await session.ExecuteReaderAsync(command, cancellationToken);
            var resultSet = new List<ResourceSet>();
            var totalCount = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var set = await session.DocumentStore.Advanced.Serializer.FromJsonAsync<ResourceSet>(
                    reader,
                    0,
                    cancellationToken);

                resultSet.Add(set);

                totalCount = Math.Max(totalCount, reader.GetInt32(1));
            }

            return cancellationToken.IsCancellationRequested
                ? new PagedResult<ResourceSet>()
                : new PagedResult<ResourceSet>
                {
                    Content = resultSet.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = totalCount
                };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{error}", ex.Message);
            return new PagedResult<ResourceSet>
            {
                Content = Array.Empty<ResourceSet>(),
                StartIndex = 0,
                TotalResults = 0
            };
        }
    }

    private static NpgsqlCommand CreateQueryCommand(SearchResourceSet parameter, ClaimsPrincipal principal, IDocumentSession session)
    {
        var builder = new StringBuilder();
        builder.Append(
            "SELECT d.data, count(d.data) over (range unbounded preceding) total from mt_doc_ownedresourceset d WHERE ");
        var parameterNames = parameter.Terms.Length == 0 ? "" : "(d.name = any (@terms) OR string_to_array(d.description, ' ') && (@terms) OR d.type = any (@terms))";
        var parameterTypes = parameter.Types.Length == 0 ? "" : "d.type = any (@type)";
        const string allowedPolicy = "(d.data @> @onlySearchScope OR d.data @> @onlyClient OR d.data @> @onlyProvider OR d.data @> @clientAndProvider)";
        const string allowedClaims = @"exists 
 (
    select policies from 
    (
			select jsonb_array_elements(d.data -> 'authorization_policies') policies
			from mt_doc_ownedresourceset d
    ) p
    where @claims @> (p.policies -> 'claims') 
 )";

        builder.AppendJoin(
            " AND ",
            new[] { parameterNames, parameterTypes, allowedPolicy, allowedClaims }.Where(
                x => !string.IsNullOrWhiteSpace(x)));
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
        var provider = principal.Claims.First(c => c.Type == "iss").Value;
        var command = new NpgsqlCommand(builder.ToString(), session.Connection);

        if (!string.IsNullOrWhiteSpace(parameterNames))
        {
            command.AddNamedParameter("terms", parameter.Terms, NpgsqlDbType.Array | NpgsqlDbType.Text);
        }

        if (!string.IsNullOrWhiteSpace(parameterTypes))
        {
            command.AddNamedParameter("type", parameter.Types, NpgsqlDbType.Array | NpgsqlDbType.Varchar);
        }

        var clientId = principal.GetClientId();
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
        command.AddNamedParameter("claims", principal.Claims.Select(ClaimData.FromClaim).ToArray(), NpgsqlDbType.Jsonb);
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