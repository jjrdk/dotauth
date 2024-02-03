namespace DotAuth.Uma;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Shared;
using Shared.Models;
using Shared.Responses;

public class HttpResourceServer(HttpClient client, ITokenCache tokenService, JsonSerializerOptions serializerOptions)
    : IUmaResourceServer
{
    /// <inheritdoc />
    public async Task<Option<ResourceResult>> GetResource(
        string resourceId,
        GrantedTokenResponse? principal,
        CancellationToken cancellationToken,
        params string[] scopes)
    {
        var accessToken = principal
         ?? await tokenService.GetToken(cancellationToken, UmaConstants.UmaProtectionScope).ConfigureAwait(false);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"resource/{resourceId}?id_token={accessToken?.IdToken}", UriKind.Relative),
            Method = HttpMethod.Get,
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken?.AccessToken) }
        };

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);
        ResourceResult? resource = response.StatusCode switch
        {
            HttpStatusCode.OK => JsonSerializer.Deserialize<ResourceDownload>(
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), serializerOptions),
            HttpStatusCode.Unauthorized => JsonSerializer.Deserialize<UmaTicketInfo>(
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), serializerOptions),
            HttpStatusCode.NotFound => new NotFoundResult(),
            _ => throw new ArgumentOutOfRangeException($"Unexpected status {response.StatusCode}")
        };
        return resource is null or UmaServerUnreachable
            ? new Option<ResourceResult>.Error(
                new ErrorDetails
                {
                    Title = "Warning",
                    Detail = "UMA Authorization Server Unreachable",
                    Status = HttpStatusCode.Forbidden
                })
            : resource;
    }
}
