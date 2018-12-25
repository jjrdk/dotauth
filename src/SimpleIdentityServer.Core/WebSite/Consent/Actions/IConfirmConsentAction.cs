namespace SimpleAuth.WebSite.Consent.Actions
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IConfirmConsentAction
    {
        Task<EndpointResult> Execute(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal, string issuerName);
    }
}