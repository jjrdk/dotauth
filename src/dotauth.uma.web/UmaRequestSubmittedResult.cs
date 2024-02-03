namespace DotAuth.Uma.Web;

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Shared.Events.Uma;
using Shared.Models;

/// <summary>
/// Defines the UMA request submitted result
/// </summary>
public class UmaRequestSubmittedResult : ObjectResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaRequestSubmittedResult"/> class.
    /// </summary>
    /// <param name="ticketId"></param>
    public UmaRequestSubmittedResult(string id, string ticketId, string clientId, params ClaimData[] claims)
        : base(new UmaRequestSubmitted(id, ticketId, clientId, claims, DateTimeOffset.UtcNow))
    {
        StatusCode = (int)HttpStatusCode.Forbidden;
    }

    /// <inheritdoc />
    public override void ExecuteResult(ActionContext context)
    {
        context.HttpContext.Response.Headers[HeaderNames.CacheControl] = CacheControlHeaderValue.NoCacheString;
        base.ExecuteResult(context);
    }

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.Headers[HeaderNames.CacheControl] = CacheControlHeaderValue.NoCacheString;
        return base.ExecuteResultAsync(context);
    }
}
