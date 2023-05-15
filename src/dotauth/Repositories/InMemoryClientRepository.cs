namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the in-memory client repository.
/// </summary>
/// <seealso cref="IClientRepository" />
internal sealed class InMemoryClientRepository : IClientRepository
{
    private readonly ILogger<InMemoryClientRepository> _logger;
    private readonly List<Client> _clients;
    private readonly ClientFactory _clientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClientRepository"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="scopeStore">The scope store.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="clients">The clients.</param>
    public InMemoryClientRepository(
        IHttpClientFactory httpClient,
        IScopeStore scopeStore,
        ILogger<InMemoryClientRepository> logger,
        IReadOnlyCollection<Client>? clients = null)
    {
        _logger = logger;
        _clientFactory = new ClientFactory(
            httpClient,
            scopeStore,
            u => JsonSerializer.Deserialize<Uri[]>(u, DefaultJsonSerializerOptions.Instance)!,
            logger);
        _clients = clients == null
            ? new List<Client>()
            : clients.ToList();
    }

    /// <inheritdoc />
    public Task<bool> Delete(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId));
        }

        var client = _clients.FirstOrDefault(c => c.ClientId == clientId);
        if (client == null)
        {
            return Task.FromResult(false);
        }

        var result = _clients.Remove(client);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<Client[]> GetAll(CancellationToken cancellationToken)
    {
        return Task.FromResult(_clients.ToArray());
    }

    /// <inheritdoc />
    public Task<Client?> GetById(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId));
        }

        var res = _clients.FirstOrDefault(c => c.ClientId == clientId);
        return res == null ? Task.FromResult<Client?>(null) : Task.FromResult<Client?>(res);
    }

    /// <inheritdoc />
    public Task<bool> Insert(Client client, CancellationToken cancellationToken = default)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (_clients.Any(x => x.ClientId == client.ClientId || x.ClientName == client.ClientName))
        {
            return Task.FromResult(false);
        }

        _clients.Add(client);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<PagedResult<Client>> Search(
        SearchClientsRequest newClient,
        CancellationToken cancellationToken = default)
    {
        if (newClient == null)
        {
            throw new ArgumentNullException(nameof(newClient));
        }


        IEnumerable<Client> result = _clients;
        if (newClient.ClientIds.Any())
        {
            result = result.Where(c => newClient.ClientIds.Any(i => c.ClientId.Contains(i)));
        }

        if (newClient.ClientNames.Any())
        {
            result = result.Where(c => newClient.ClientNames.Any(n => c.ClientName.Contains(n)));
        }

        if (newClient.ClientTypes.Any())
        {
            var clientTypes = newClient.ClientTypes.Select(t => t).ToHashSet();
            result = result.Where(c => clientTypes.Contains(c.ApplicationType))
                .OrderBy(c => c.ClientName);
        }

        var resultArray = result.ToArray();
        var nbResult = resultArray.Length;

        if (newClient.NbResults > 0)
        {
            resultArray = resultArray.Skip(newClient.StartIndex).Take(newClient.NbResults).ToArray();
        }

        return Task.FromResult(
            new PagedResult<Client>
            {
                Content = resultArray,
                StartIndex = newClient.StartIndex,
                TotalResults = nbResult
            });
    }

    /// <inheritdoc />
    public async Task<Option> Update(Client newClient, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newClient.ClientId)
         || !_clients.Exists(x => x.ClientId == newClient.ClientId))
        {
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidClient,
                    Detail = SharedStrings.TheClientDoesntExist,
                    Status = HttpStatusCode.NotFound
                });
        }

        var option = await _clientFactory.Build(newClient, false, cancellationToken).ConfigureAwait(false);
        if (option is Option<Client>.Error e)
        {
            return new Option.Error(e.Details, e.State);
        }

        newClient = ((Option<Client>.Result)option).Item;
        lock (_clients)
        {
            var removed = _clients.RemoveAll(
                x => x.ClientId == newClient.ClientId || x.ClientName == newClient.ClientName);
            if (removed != 1)
            {
                _logger.LogError("Client {clientId} not properly updated.", newClient.ClientId);
            }

            _clients.Add(newClient);
        }

        return new Option.Success();
    }
}
