namespace DotAuth.Uma;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Client;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.Extensions.Logging;
using Shared.Models;

public class ResourceClient : IResourceClient
{
    //private const string RequestSubmitted = "request_submitted";
    private readonly IUmaResourceServer _resourceServer;
    private readonly ITokenClient _tokenClient;
    private readonly IUmaPermissionClient _permissionClient;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ResourceClient> _logger;

    public ResourceClient(
        IUmaResourceServer resourceServer,
        ITokenClient tokenClient,
        IUmaPermissionClient permissionClient,
        HttpClient httpClient,
        JsonSerializerOptions serializerOptions,
        ILogger<ResourceClient> logger)
    {
        _resourceServer = resourceServer;
        _tokenClient = tokenClient;
        _permissionClient = permissionClient;
        _httpClient = httpClient;
        _serializerOptions = serializerOptions;
        _logger = logger;
    }

    public async Task<Option<ResourceResult>> Get(
        string resourceId,
        ResourceOwnerInfo principal,
        CancellationToken cancellationToken = default,
        params string[] scopes)
    {
        var permission = principal.GetExistingPermission(resourceId, scopes);
        if (permission != null)
        {
            return await _resourceServer.GetResource(resourceId, permission, cancellationToken, scopes);
        }

        ResourceResult requestTicket =
            principal.GetExistingRequest(principal.Subject, resourceId, out var request, scopes)
                ? new UmaTicketInfo(request!.TicketId, _permissionClient.Authority.AbsoluteUri)
                : new UmaServerUnreachable();
        switch (requestTicket)
        {
            case UmaTicketInfo ticketInfo:
                {
                    var tokenOption = await _tokenClient.GetToken(
                            TokenRequest.FromTicketId(ticketInfo.TicketId, principal.IdToken!),
                            cancellationToken)
                        .ConfigureAwait(false);
                    switch (tokenOption)
                    {
                        case Option<GrantedTokenResponse>.Result umaToken:
                            _logger.LogInformation("Permission token received");
                            _ = principal.ResourceRequests.RemoveAll(r => r.TicketId == ticketInfo.TicketId);
                            principal.PermissionRegistrations.Add(new PermissionRegistration(resourceId, umaToken.Item));
                            var result = await _resourceServer
                                .GetResource(resourceId, umaToken.Item, cancellationToken, scopes)
                                .ConfigureAwait(false);
                            if (result is Option<ResourceResult>.Result { Item: UmaTicketInfo umaTicketInfo })
                            {
                                principal.ResourceRequests.Add(
                                    new(principal.Subject, resourceId, scopes, umaTicketInfo.TicketId));
                            }

                            return result;
                        case Option<GrantedTokenResponse>.Error tokenError:
                            _logger.LogError("{Error}", tokenError.Details.Title);
                            return tokenError.Details;
                        default: throw new Exception();
                    }
                }
            case UmaServerUnreachable:
                {
                    var resourceOption = await _resourceServer.GetResource(
                            resourceId,
                            new GrantedTokenResponse
                            {
                                AccessToken = principal.Pat!,
                                IdToken = principal.IdToken,
                                RefreshToken = principal.RefreshToken
                            },
                            cancellationToken,
                            scopes)
                        .ConfigureAwait(false);
                    if (resourceOption is Option<ResourceResult>.Result { Item: UmaTicketInfo umaTicketInfo })
                    {
                        principal.ResourceRequests.Add(new(principal.Subject, resourceId, scopes, umaTicketInfo.TicketId));
                    }

                    return resourceOption;
                }
            default: throw new Exception();
        }
    }

    /// <inheritdoc />
    public async Task<Option<PagedResult<ResourceDescription>>> Search(string[] search, string idToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"https://uma.reimers.dk/search?q={string.Join('+', search)}")
        };
        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return response.IsSuccessStatusCode switch
        {
            false => JsonSerializer.Deserialize<ErrorDetails>(await response.Content.ReadAsStringAsync(cancellationToken), _serializerOptions)!,
            true => JsonSerializer.Deserialize<PagedResult<ResourceDescription>>(await response.Content.ReadAsStringAsync(cancellationToken), _serializerOptions)!
        };
    }
}
