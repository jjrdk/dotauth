namespace DotAuth.Uma;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using Shared;
using Shared.Models;
using Shared.Responses;

public class UmaPermissionManagementClient : ClientBase, IUmaPermissionManagementClient
{
    private readonly IUmaResourceSetClient _resourceSetClient;
    private readonly Uri _authority;
    private readonly IResourceOwnerInfoStore _resourceOwnerInfoStore;
    private UmaConfiguration? _umaConfiguration;

    /// <inheritdoc />
    public UmaPermissionManagementClient(
        Func<HttpClient> client,
        IUmaResourceSetClient resourceSetClient,
        Uri authority,
        IResourceOwnerInfoStore resourceOwnerInfoStore)
        : base(client, authority)
    {
        _resourceSetClient = resourceSetClient;
        _authority = authority;
        _resourceOwnerInfoStore = resourceOwnerInfoStore;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccessRequestDescription>> GetOpenRequests(
        string subject,
        CancellationToken cancellationToken = default)
    {
        var resourceOwner = await _resourceOwnerInfoStore.Get(subject, cancellationToken).ConfigureAwait(false);
        var config = await GetUmaConfiguration(cancellationToken);
        var requestMessage = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = config.PermissionEndpoint };
        var accessToken = resourceOwner!.Pat;
        var result = await GetResult<Ticket[]>(
                requestMessage,
                new AuthenticationHeaderValue("Bearer", accessToken!),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result is Option<Ticket[]>.Error e)
        {
            throw new Exception(e.Details.Detail);
        }

        var resultList = new List<AccessRequestDescription>();
        if (result is not Option<Ticket[]>.Result r)
        {
            return resultList;
        }

        foreach (var ticket in r.Item)
        {
            await foreach (var description in GetDescriptions(ticket, null!, cancellationToken))
            {
                resultList.Add(description);
            }
        }

        return resultList;
    }

    private async IAsyncEnumerable<AccessRequestDescription> GetDescriptions(
        Ticket ticket,
        string token,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tasks = ticket.Lines.Select(
                ticketLine => (ticketLine.Scopes,
                               _resourceSetClient.GetResourceSet(ticketLine.ResourceSetId, token, cancellationToken)))
            .ToArray();
        _ = await Task.WhenAll(tasks.Select(x => x.Item2)).ConfigureAwait(false);
        var resources = tasks.Select(x => (x.Scopes, x.Item2.Result))
            .Where(o => o.Result is Option<ResourceSet>.Result)
            .Select(x => (x.Scopes, (x.Result as Option<ResourceSet>.Result)!.Item))
            .Select(
                x => new ResourceAccessDescription
                {
                    ResourceSetId = x.Item.Id,
                    Description = x.Item.Description,
                    IconUri = x.Item.IconUri,
                    Name = x.Item.Name,
                    ResourceType = x.Item.Type,
                    Scopes = x.Scopes
                })
            .ToArray();

        yield return new AccessRequestDescription
        {
            TicketId = ticket.Id,
            Created = ticket.Created,
            Expires = ticket.Expires,
            RequesterName =
                ticket.Requester.FirstOrDefault(x => x.Type == OpenIdClaimTypes.Name)?.Value ?? string.Empty,
            RequesterEmail = ticket.Requester.FirstOrDefault(x => x.Type == OpenIdClaimTypes.Email)?.Value
             ?? string.Empty,
            RequestedResources = resources
        };
    }

    /// <inheritdoc />
    public Task<bool> Approve(string ticketId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<UmaConfiguration> GetUmaConfiguration(CancellationToken cancellationToken)
    {
        if (_umaConfiguration != null)
        {
            return _umaConfiguration!;
        }

        var configurationUri = new Uri(_authority, ".well-known/uma2-configuration");
        var result = await GetResult<UmaConfiguration>(
                new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = configurationUri },
                (AuthenticationHeaderValue?)null,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        switch (result)
        {
            case Option<UmaConfiguration>.Error e:
                throw new Exception(e.Details.Detail);
            case Option<UmaConfiguration>.Result r:
                _umaConfiguration = r.Item;
                return _umaConfiguration!;
            default:
                throw new Exception("Unknown result type");
        }
    }
}
