namespace SimpleAuth.AcceptanceTests
{
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Client;
    using SimpleAuth.ResourceServer;
    using SimpleAuth.Shared.Requests;

    [Route("[controller]")]
    public class DataController : ControllerBase
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
            if (userIdentity.TryGetUmaTickets(out var permissions) && permissions.Any(x => x.ResourceSetId == id))
            {
                return Ok("Hello");
            }

            var token = await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false);
            var request = new PermissionRequest { ResourceSetId = id, Scopes = new[] { "api1" } };
            var ticket = await _umaClient.RequestPermission(token, cancellationToken, request).ConfigureAwait(false);
            return new UmaTicketResult(new UmaTicketResult.UmaTicketInfo(ticket.Content.TicketId, _umaClient.Authority.AbsoluteUri));
        }
    }
}