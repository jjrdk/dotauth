namespace SimpleAuth.AcceptanceTests
{
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.ResourceServer;

    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet("{id}")]
        [Authorize("uma_ticket")]
        public ActionResult<string> Index(string id)
        {
            var userIdentity = User.Identity as ClaimsIdentity;
            if (userIdentity.TryGetUmaTickets(out var lines) && lines.Any(x=>x.ResourceSetId == id))
            {
                return "Hello";
            }

            return StatusCode(412);
        }
    }
}