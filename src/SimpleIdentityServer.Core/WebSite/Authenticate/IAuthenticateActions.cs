namespace SimpleIdentityServer.Core.WebSite.Authenticate
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Actions;
    using Parameters;
    using Results;

    public interface IAuthenticateActions
    {
        Task<ActionResult> AuthenticateResourceOwnerOpenId(AuthorizationParameter parameter, ClaimsPrincipal claimsPrincipal, string code, string issuerName);
        Task<LocalOpenIdAuthenticationResult> LocalOpenIdUserAuthentication(LocalAuthenticationParameter localAuthenticationParameter, AuthorizationParameter authorizationParameter, string code, string issuerName);
        Task<string> GenerateAndSendCode(string subject);
        Task<bool> ValidateCode(string code);
        Task<bool> RemoveCode(string code);
    }
}