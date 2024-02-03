namespace DotAuth.Uma.Web;

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
    protected override Task ExecuteResult(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = (int)HttpStatusCode.Forbidden;
        response.Headers[HeaderNames.CacheControl] = CacheControlHeaderValue.NoCacheString;
        response.Headers[HeaderNames.Warning] = UmaAuthorizationServerUnreachable;

        return Task.CompletedTask;
    }
}