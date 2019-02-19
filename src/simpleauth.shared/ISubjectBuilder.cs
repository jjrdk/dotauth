namespace SimpleAuth.Shared
{
    using System.Collections.Generic;
    using System.Security.Claims;
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
        /// <returns></returns>
        Task<string> BuildSubject(IEnumerable<Claim> claims);
    }
}