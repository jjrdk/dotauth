namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the resource owner store interface.
    /// </summary>
    public interface IResourceOwnerStore
    {
        /// <summary>
        /// Gets the resource owner by claim.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ResourceOwner> GetResourceOwnerByClaim(string key, string value, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ResourceOwner> Get(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified external account.
        /// </summary>
        /// <param name="externalAccount">The external account.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ResourceOwner> Get(ExternalAccountLink externalAccount, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ResourceOwner> Get(string id, string password, CancellationToken cancellationToken);
    }
}