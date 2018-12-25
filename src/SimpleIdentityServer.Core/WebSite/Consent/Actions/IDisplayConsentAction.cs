namespace SimpleAuth.WebSite.Consent.Actions
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;

    public interface IDisplayConsentAction
    {
        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the parameter
        /// Those informations are used to create the consent screen.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant type parameter.</param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="originUrl"></param>
        /// <returns>Action result.</returns>
        Task<DisplayContentResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal, string issuerName);
    }
}