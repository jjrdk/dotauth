namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    using System.Threading.Tasks;
    using Parameters;

    public interface ILocalOpenIdUserAuthenticationAction
    {
        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        Task<LocalOpenIdAuthenticationResult> Execute(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code, string issuerName);
    }
}