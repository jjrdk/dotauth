namespace SimpleAuth
{
    using System.Threading.Tasks;
    using Shared.Models;

    /// <summary>
    /// Defines the authorization code store interface.
    /// </summary>
    public interface IAuthorizationCodeStore
    {
        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<AuthorizationCode> GetAuthorizationCode(string code);

        /// <summary>
        /// Adds the authorization code.
        /// </summary>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <returns></returns>
        Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode);

        /// <summary>
        /// Removes the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<bool> RemoveAuthorizationCode(string code);
    }
}
