namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the consent store interface.
    /// </summary>
    public interface IConsentStore
    {
        /// <summary>
        /// Gets the consents for given user.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<Consent>> GetConsentsForGivenUser(
            string subject,
            CancellationToken cancellationToken);
    }
}
