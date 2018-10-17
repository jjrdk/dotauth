namespace SimpleIdentityServer.Core.Api.Authorization.Common
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Common.Models;
    using Parameters;
    using Results;

    public interface IProcessAuthorizationRequest
    {
        Task<ActionResult> ProcessAsync(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName);
    }
}