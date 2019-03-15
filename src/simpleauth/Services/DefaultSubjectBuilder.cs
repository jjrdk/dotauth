namespace SimpleAuth.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared;

    internal class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject(IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Id.Create());
        }
    }
}