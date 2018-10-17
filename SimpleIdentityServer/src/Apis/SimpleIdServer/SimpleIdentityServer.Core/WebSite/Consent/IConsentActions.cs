namespace SimpleIdentityServer.Core.WebSite.Consent
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Actions;
    using Parameters;
    using Results;

    public interface IConsentActions
    {
        Task<DisplayContentResult> DisplayConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
        Task<ActionResult> ConfirmConsent(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
    }
}