namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Stores.Marten.Containers;
using global::Marten;
using global::Marten.Internal;
using global::Marten.Pagination;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the marten based client store.
/// </summary>
/// <seealso cref="IClientRepository" />
public sealed class MartenClientStore : IClientRepository
{
    private readonly Func<IDocumentSession> _sessionFactory;
    private readonly ILogger<MartenClientStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenClientStore"/> class.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    /// <param name="logger">The <see cref="ILogger{T}"/> to use.</param>
    public MartenClientStore(Func<IDocumentSession> sessionFactory, ILogger<MartenClientStore> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Client?> GetById(string clientId, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var client = await session.Query<Client>()
            .Where(x => x.ClientId == clientId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (client != null)
        {
            return client;
        }

        if (session is IMartenSession martenSession)
        {
            _logger.LogWarning("Client {clientId} not found in tenant {tenant}", clientId, martenSession.TenantId);
        }
        else
        {
            _logger.LogWarning("Client {clientId} not found", clientId);
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<Client[]> GetAll(CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var clients = await session.Query<Client>().ToListAsync(cancellationToken).ConfigureAwait(false);
        return clients.ToArray();
    }

    /// <inheritdoc />
    public async Task<PagedResult<Client>> Search(
        SearchClientsRequest parameter,
        CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var take = parameter.NbResults == 0 ? int.MaxValue : parameter.NbResults;
        var results = await session.Query<Client>()
            .Where(x => x.ClientId.IsOneOf(parameter.ClientIds))
            .ToPagedListAsync(parameter.StartIndex + 1, take, cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<Client>
        {
            Content = results.ToArray(),
            StartIndex = parameter.StartIndex,
            TotalResults = results.TotalItemCount
        };
    }

    /// <inheritdoc />
    public async Task<Option> Update(Client client, CancellationToken cancellationToken)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var existing = await session.LoadAsync<Client>(client.ClientId, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null)
        {
            return new ErrorDetails
            {
                Title = ErrorCodes.InvalidClient,
                Detail = SharedStrings.TheClientDoesntExist,
                Status = HttpStatusCode.NotFound
            };
        }
        
        session.Update(client);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new Option.Success();
    }

    /// <inheritdoc />
    public async Task<bool> Insert(Client client, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var existing = await session.LoadAsync<Client>(client.ClientId, cancellationToken)
            .ConfigureAwait(false);
        if (existing != null)
        {
            return false;
        }
        
        session.Store(client);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> Delete(string clientId, CancellationToken cancellationToken = default)
    {
        var session = _sessionFactory();
        await using var _ = session.ConfigureAwait(false);
        var existing = await session.Query<Client>().Where(x => x.ClientId == clientId).Select(x => x.ClientId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(existing))
        {
            return false;
        }

        session.Delete<Client>(existing);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;

    }
}
