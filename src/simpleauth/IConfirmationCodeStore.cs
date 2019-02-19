namespace SimpleAuth
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the confirmation code store.
    /// </summary>
    public interface IConfirmationCodeStore
    {
        /// <summary>
        /// Gets the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<ConfirmationCode> Get(string code);

        /// <summary>
        /// Adds the specified confirmation code.
        /// </summary>
        /// <param name="confirmationCode">The confirmation code.</param>
        /// <returns></returns>
        Task<bool> Add(ConfirmationCode confirmationCode);

        /// <summary>
        /// Removes the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Task<bool> Remove(string code);
    }
}
