using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.Manager.Host.Extensions;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.Configuration)]
    public class ConfigurationController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                ClaimsEndpoint = issuer + Constants.EndPoints.Claims,
                ClientsEndpoint = issuer + Constants.EndPoints.Clients,
                JweEndpoint = issuer + Constants.EndPoints.Jwe,
                JwsEndpoint = issuer + Constants.EndPoints.Jws,
                ManageEndpoint = issuer + Constants.EndPoints.Manage,
                ResourceOwnersEndpoint = issuer + Constants.EndPoints.ResourceOwners,
                ScopesEndpoint = issuer + Constants.EndPoints.Scopes
            };
            return new OkObjectResult(result);
        }
    }
}
