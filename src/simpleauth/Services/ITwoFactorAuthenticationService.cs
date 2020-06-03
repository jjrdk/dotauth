namespace SimpleAuth.Services
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the two factor authentication service.
    /// </summary>
    public interface ITwoFactorAuthenticationService
    {
        /// <summary>
        /// Sends the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        Task<(bool, string)> Send(string code, ResourceOwner user);

        /// <summary>
        /// Gets the required claim.
        /// </summary>
        /// <value>
        /// The required claim.
        /// </value>
        string RequiredClaim { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
    }
}
