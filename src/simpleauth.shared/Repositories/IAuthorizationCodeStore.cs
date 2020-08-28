namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the authorization code store interface.
    /// </summary>
    public interface IAuthorizationCodeStore
    {
        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<AuthorizationCode?> Get(string code, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the authorization code.
        /// </summary>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<bool> Add(AuthorizationCode authorizationCode, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the authorization code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<bool> Remove(string code, CancellationToken cancellationToken);
    }
}
