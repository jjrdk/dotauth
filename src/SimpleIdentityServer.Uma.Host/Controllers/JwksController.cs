using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    using SimpleAuth.Api.Jwks;
    using SimpleAuth.Shared.Requests;

    [Route(Constants.RouteValues.Jwks)]
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
