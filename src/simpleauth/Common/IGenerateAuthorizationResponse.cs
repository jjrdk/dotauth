namespace SimpleAuth.Common
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;
    using Shared.Models;

    public interface IGenerateAuthorizationResponse
    {
        Task Generate(
            EndpointResult endpointResult,
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            Client client,
            string issuerName);
    }
}
