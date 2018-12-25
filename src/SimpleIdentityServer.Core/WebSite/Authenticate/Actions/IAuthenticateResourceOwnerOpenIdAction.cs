namespace SimpleAuth.WebSite.Authenticate.Actions
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IAuthenticateResourceOwnerOpenIdAction
    {
        /// <summary>
        /// Returns an action result to the controller's action.
        /// 1). Redirect to the consent screen if the user is authenticated AND the request doesn't contain a login prompt.
        /// 2). Do nothing
        /// </summary>
        /// <param name="authorizationParameter">The parameter</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted parameter</param>
        /// <returns>Action result to the controller's action</returns>
        Task<EndpointResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code, string issuerName);
    }
}