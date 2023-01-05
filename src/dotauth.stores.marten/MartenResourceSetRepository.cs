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
using DotAuth.Shared.Policies;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using global::Marten;
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
    private readonly IAuthorizationPolicy _authorizationPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenScopeRepository"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    /// <param name="authorizationPolicy">The active <see cref="IAuthorizationPolicy"/>.</param>
    public MartenResourceSetRepository(Func<IDocumentSession> sessionFactory, IAuthorizationPolicy authorizationPolicy)
    {
        _sessionFactory = sessionFactory;
        _authorizationPolicy = authorizationPolicy;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ResourceSet>> Search(
        ClaimsPrincipal requestor,
        SearchResourceSet parameter,
        CancellationToken cancellationToken)
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
            if ((await _authorizationPolicy.Execute(
                    new TicketLineParameter(requestor.GetClientId(), new[] { "search" }),
                    UmaConstants.IdTokenType,
                    requestor,
                    cancellationToken,
                    set.AuthorizationPolicies)).Result
                == AuthorizationPolicyResultKind.Authorized)
            {
                resultSet.Add(set);
            }

            totalCount = Math.Max(totalCount, reader.GetInt32(1));
        }

        return cancellationToken.IsCancellationRequested
            ? new PagedResult<ResourceSet>()
            : new PagedResult<ResourceSet>
            {
                Content = resultSet.ToArray(),
                StartIndex = parameter.StartIndex,
                TotalResults = resultSet.Count //TotalItemCount
            };
    }

    private static NpgsqlCommand CreateQueryCommand(SearchResourceSet parameter, ClaimsPrincipal principal, IDocumentSession session)
    {
        var builder = new StringBuilder();
        builder.Append(
            @"SELECT d.data, count(d.data) over (range unbounded preceding) total from mt_doc_ownedresourceset d WHERE ");
        var parameterNames = parameter.Terms.Length == 0 ? "" : "(@terms @> (data ->> 'name') OR @terms @> (data ->> 'description'))";
        var parameterTypes = parameter.Types.Length == 0 ? "" : "@type @> (data ->> 'type')";
        const string allowedPolicy = "d.data @> {'authorization_policies':[{'clients': [@client], 'scopes': ['search'], 'provider': @provider}]}";
        const string allowedClaims = "d.data <@ {'authorization_policies':[{'claims':[@claims]}]}";

        builder.AppendJoin(
            " AND ",
            new[] { parameterNames, parameterTypes, allowedPolicy, allowedClaims }.Where(
                x => !string.IsNullOrWhiteSpace(x)));
        builder.Append(" ORDER BY (d.data ->> 'name')");
        if (parameter.TotalResults > 0)
        {
            builder.Append($" LIMIT {parameter.TotalResults}");
        }

        if (parameter.StartIndex > 0)
        {
            builder.Append($" OFFSET {parameter.StartIndex}");
        }

        var command = new NpgsqlCommand(builder.ToString(), session.Connection);
        
        if (!string.IsNullOrWhiteSpace(parameterNames))
        {
            command.AddNamedParameter("terms", parameter.Terms, NpgsqlDbType.Jsonb);
        }

        if (!string.IsNullOrWhiteSpace(parameterTypes))
        {
            command.AddNamedParameter("type", parameter.Types, NpgsqlDbType.Jsonb);
        }
        command.AddNamedParameter("client", principal.GetClientId(), NpgsqlDbType.Varchar);
        command.AddNamedParameter("provider", DBNull.Value, NpgsqlDbType.Varchar);
        command.AddNamedParameter("claims", principal.Claims.Select(ClaimData.FromClaim).ToArray(), NpgsqlDbType.Jsonb);
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