namespace DotAuth.Uma.Web;

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Defines the UMA server unreachable result.
/// </summary>
public class UmaServerUnreachableResult : UmaResult<string>
{
    private const string UmaAuthorizationServerUnreachable = "199 - \"UMA Authorization Server Unreachable\"";

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaServerUnreachableResult"/> class.
    /// </summary>
    public UmaServerUnreachableResult() : base(UmaAuthorizationServerUnreachable)
    {
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(HttpContext context)
    {
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.Forbidden;
        response.Headers[HeaderNames.CacheControl] = CacheControlHeaderValue.NoCacheString;
        response.Headers[HeaderNames.Warning] = UmaAuthorizationServerUnreachable;

        return Task.CompletedTask;
    }
}
