namespace SimpleIdentityServer.Core.Common
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Parameters;
    using Core.Results;
    using Models;

    public interface IGenerateAuthorizationResponse
    {
        Task ExecuteAsync(ActionResult actionResult, AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, Client client, string issuerName);
    }
}