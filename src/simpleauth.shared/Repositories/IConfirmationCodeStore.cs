namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the confirmation code store.
    /// </summary>
    public interface IConfirmationCodeStore
    {
        /// <summary>
        /// Gets the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ConfirmationCode> Get(string code, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the specified confirmation code.
        /// </summary>
        /// <param name="confirmationCode">The confirmation code.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Add(ConfirmationCode confirmationCode, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Remove(string code, CancellationToken cancellationToken);
    }
}
