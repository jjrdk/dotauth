namespace DotAuth.AcceptanceTests.Support;

using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

[Route("[controller]")]
public sealed class DataController : ControllerBase
{
    private readonly UmaClient _umaClient;

    public DataController(UmaClient umaClient)
    {
        _umaClient = umaClient;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Index(string id, CancellationToken cancellationToken)
    {
        var userIdentity = User.Identity as ClaimsIdentity;
        var umaTickets = userIdentity!.TryGetUmaTickets(out var permissions);
        if (umaTickets && permissions.Any(x => x.ResourceSetId == id))
        {
            return Ok("Hello");
        }

        var token = await HttpContext.GetTokenAsync("access_token");
        var request = new PermissionRequest {ResourceSetId = id, Scopes = new[] {"api1"}};
        var option = await _umaClient.RequestPermission(token!, cancellationToken, request);
        if (option is Option<TicketResponse>.Result ticket)
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            Response.Headers[HeaderNames.WWWAuthenticate] =
                $"UMA as_uri=\"{_umaClient.Authority.AbsoluteUri}\", ticket=\"{ticket.Item.TicketId}\"";

            return StatusCode((int)HttpStatusCode.Unauthorized);
        }

        return BadRequest(option as Option<TicketResponse>.Error);
    }
}
