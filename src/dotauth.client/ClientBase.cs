namespace DotAuth.Client;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the base client for interacting with an authorization server.
/// </summary>
public abstract class ClientBase
{
    private DiscoveryInformation? _discovery;
    private readonly GetDiscoveryOperation? _discoveryOperation;

    /// <summary>
    /// The mime type for json requests.
    /// </summary>
    protected const string JsonMimeType = "application/json";

    private readonly Func<HttpClient> _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBase"/> class.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="authority">The base <see cref="Uri"/> of the authorization authority.</param>
    protected ClientBase(Func<HttpClient> client, Uri authority)
    {
        _client = client;
        _discoveryOperation = new GetDiscoveryOperation(authority, client);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBase"/> class.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="discoveryDocumentation">The metadata information.</param>
    protected ClientBase(Func<HttpClient> client, DiscoveryInformation discoveryDocumentation)
    {
        _client = client;
        _discovery = discoveryDocumentation;
    }

    /// <summary>
    /// Gets the result of the <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of data downloaded.</typeparam>
    /// <param name="request">The download request.</param>
    /// <param name="token">The authorization token for the request.</param>
    /// <param name="certificate">The <see cref="X509Certificate2"/> to include in request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    protected Task<Option<T>> GetResult<T>(
        HttpRequestMessage request,
        string? token,
        X509Certificate2? certificate = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return GetResult<T>(
            request,
            token == null ? null : new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, token),
            certificate,
            cancellationToken);
    }

    /// <summary>
    /// Gets the result of the <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of data downloaded.</typeparam>
    /// <param name="request">The download request.</param>
    /// <param name="token">The authorization token for the request.</param>
    /// <param name="certificate">The <see cref="X509Certificate2"/> to include in request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    protected async Task<Option<T>> GetResult<T>(
        HttpRequestMessage request,
        AuthenticationHeaderValue? token,
        X509Certificate2? certificate = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        request = PrepareRequest(request, token, certificate);
        var result = await _client().SendAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsSuccessStatusCode && result.StatusCode != HttpStatusCode.NoContent)
        {
            return Serializer.Default.Deserialize<T>(content)!;
        }

        var genericResult = string.IsNullOrWhiteSpace(content)
            ? new ErrorDetails { Status = result.StatusCode }
            : Serializer.Default.Deserialize<ErrorDetails>(content);

        return genericResult!;
    }

    /// <summary>
    /// Gets the discovery information for a given authority.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>A <see cref="DiscoveryInformation"/> document.</returns>
    protected async Task<DiscoveryInformation> GetDiscoveryInformation(CancellationToken cancellationToken = default)
    {
        return _discovery ??= await _discoveryOperation!.Execute(cancellationToken)
            .ConfigureAwait(false);
    }

    private static HttpRequestMessage PrepareRequest(
        HttpRequestMessage request,
        AuthenticationHeaderValue? authorizationHeaderValue,
        X509Certificate2? certificate = null)
    {
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));
        request.Headers.Authorization = authorizationHeaderValue;

        if (certificate != null)
        {
            var base64Encoded = Convert.ToBase64String(certificate.RawData);
            request.Headers.Add("X-ARR-ClientCert", base64Encoded);
        }

        return request;
    }
}
