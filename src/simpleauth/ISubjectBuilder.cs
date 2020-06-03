namespace SimpleAuth
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the subject builder interface.
    /// </summary>
    public interface ISubjectBuilder
    {
        /// <summary>
        /// Builds the subject.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<string> BuildSubject(IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
    }
}