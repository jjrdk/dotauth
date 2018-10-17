namespace SimpleIdentityServer.Core.Api.Authorization.Actions
{
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Core.Common.Models;
    using Parameters;
    using Results;

    public interface IGetAuthorizationCodeOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal claimsPrincipal, Client client, string issuerName);
    }
}