namespace SimpleAuth.Api.Authorization.Actions
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Parameters;
    using Results;
    using Shared.Models;

    public interface IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation
    {
        Task<EndpointResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal claimsPrincipal, Client client, string issuerName);
    }
}