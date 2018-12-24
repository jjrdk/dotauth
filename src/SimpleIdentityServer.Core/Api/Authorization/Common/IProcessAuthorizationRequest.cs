namespace SimpleIdentityServer.Core.Api.Authorization.Common
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;
    using SimpleAuth.Shared.Models;

    public interface IProcessAuthorizationRequest
    {
        Task<EndpointResult> ProcessAsync(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName);
    }
}