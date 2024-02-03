namespace DotAuth.Uma.Web;

using System.Net;
using System.Threading.Tasks;
using DotAuth.Uma;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Defines the UMA ticket result class.
/// </summary>
public class UmaTicketResult : UmaResult<UmaTicketInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmaTicketResult"/> class.
    /// </summary>
    /// <param name="info">The <see cref="UmaTicketInfo"/> to return to the client.</param>
    public UmaTicketResult(UmaTicketInfo info) : base(info)
    {
    }

    /// <inheritdoc />
    protected override Task ExecuteResult(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = (int)HttpStatusCode.Unauthorized;
        var s = string.IsNullOrWhiteSpace(Value.Realm) ? string.Empty : $"realm=\"{Value.Realm}\", ";
        response.Headers[HeaderNames.WWWAuthenticate] =
            $"UMA {s}as_uri=\"{Value.UmaAuthority}\", ticket=\"{Value.TicketId}\"";

        return Task.CompletedTask;
    }
}
