namespace SimpleAuth.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the default subject builder type.
    /// </summary>
    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        /// <summary>
        /// Builds a subject identifier based on the passed claims.
        /// </summary>
        /// <param name="claims">The claims information.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>A subject as a <see cref="string"/>.</returns>
        public Task<string> BuildSubject(IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Id.Create());
        }
    }
}