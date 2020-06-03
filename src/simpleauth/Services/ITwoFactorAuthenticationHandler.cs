namespace SimpleAuth.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the two factor authentication handler interface.
    /// </summary>
    public interface ITwoFactorAuthenticationHandler
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITwoFactorAuthenticationService> GetAll();

        /// <summary>
        /// Gets the specified two factor authentication type.
        /// </summary>
        /// <param name="twoFactorAuthType">Type of the two factor authentication.</param>
        /// <returns></returns>
        ITwoFactorAuthenticationService Get(string twoFactorAuthType);

        /// <summary>
        /// Sends the code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="twoFactorAuthType">Type of the two factor authentication.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        Task<bool> SendCode(string code, string twoFactorAuthType, ResourceOwner user);
    }
}