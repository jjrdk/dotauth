namespace SimpleAuth.Shared.Repositories
{
    using Microsoft.IdentityModel.Tokens;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the JSON Web Key Set repository interface
    /// </summary>
    public interface IJwksRepository : IJwksStore
    {
        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<bool> Add(JsonWebKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rotates the specified key set.
        /// </summary>
        /// <param name="keySet">The key set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<bool> Rotate(JsonWebKeySet keySet, CancellationToken cancellationToken = default);
    }
}