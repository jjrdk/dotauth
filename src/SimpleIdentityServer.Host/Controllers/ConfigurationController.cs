namespace SimpleIdentityServer.Host.Controllers
{
    using Core;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Responses;

    [Route(HostEnpoints.Configuration)]
    public class ConfigurationController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                ClaimsEndpoint = issuer + HostEnpoints.Claims,
                ClientsEndpoint = issuer + HostEnpoints.Clients,
                JweEndpoint = issuer + HostEnpoints.Jwe,
                JwsEndpoint = issuer + HostEnpoints.Jws,
                ManageEndpoint = issuer + HostEnpoints.Manage,
                ResourceOwnersEndpoint = issuer + HostEnpoints.ResourceOwners,
                ScopesEndpoint = issuer + HostEnpoints.Scopes
            };
            return new OkObjectResult(result);
        }
    }
}
