namespace SimpleAuth.Api.Authorization.Common
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;
    using Shared.Models;

    public interface IProcessAuthorizationRequest
    {
        Task<EndpointResult> ProcessAsync(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName);
    }
}