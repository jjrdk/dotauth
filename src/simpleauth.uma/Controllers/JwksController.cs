namespace SimpleAuth.Uma.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Api.Jwks;
    using SimpleAuth.Shared.Requests;

    [Route(UmaConstants.RouteValues.Jwks)]
    public class JwksController : Controller
    {
        private readonly IJwksActions _jwksActions;

        public JwksController(IJwksActions jwksActions)
        {
            _jwksActions = jwksActions;
        }

        [HttpGet]
        public async Task<JsonWebKeySet> Get()
        {
            return await _jwksActions.GetJwks().ConfigureAwait(false);
        }

        [HttpPut]
        public async Task<bool> Put()
        {
            return await _jwksActions.RotateJwks().ConfigureAwait(false);
        }
    }
}
