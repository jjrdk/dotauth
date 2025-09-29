namespace DotAuth.Client;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the client for interacting with the dynamic client registration endpoint.
/// </summary>
public sealed class DynamicRegistrationClient : ClientBase, IDynamicRegistrationClient
{
    /// <inheritdoc />
    public DynamicRegistrationClient(Func<HttpClient> client, Uri authority)
        : base(client, authority)
    {
    }

    /// <inheritdoc />
    public async Task<Option<DynamicClientRegistrationResponse>> Register(
        string accessToken,
        DynamicClientRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var discovery = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = discovery.DynamicClientRegistrationEndpoint,
            Content = new StringContent(
                JsonSerializer.Serialize(request, SharedSerializerContext.Default.DynamicClientRegistrationRequest),
                Encoding.UTF8,
                "application/json")
        };

        return await GetResult<DynamicClientRegistrationResponse>(
                msg,
                new AuthenticationHeaderValue("Bearer", accessToken),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Option<DynamicClientRegistrationResponse>> Modify(
        string accessToken,
        string clientId,
        DynamicClientRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var discovery = await GetDiscoveryInformation(cancellationToken).ConfigureAwait(false);
        var msg = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(discovery.DynamicClientRegistrationEndpoint, $"register/{clientId}"),
            Content = new StringContent(JsonSerializer.Serialize(request, SharedSerializerContext.Default.DynamicClientRegistrationRequest),
                Encoding.UTF8, "application/json")
        };

        return await GetResult<DynamicClientRegistrationResponse>(
                msg,
                new AuthenticationHeaderValue("Bearer", accessToken),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
