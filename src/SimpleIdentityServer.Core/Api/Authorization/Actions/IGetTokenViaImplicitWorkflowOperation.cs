namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Core.Common.Models;
    using Parameters;
    using Results;

    public interface IGetTokenViaImplicitWorkflowOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Client client, string issuerName);
    }
}