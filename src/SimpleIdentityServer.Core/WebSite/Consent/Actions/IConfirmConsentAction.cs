namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IConfirmConsentAction
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
    }
}