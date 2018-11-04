namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Parameters;
    using Results;
    using Shared.Models;

    public interface IGetTokenViaImplicitWorkflowOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Client client, string issuerName);
    }
}