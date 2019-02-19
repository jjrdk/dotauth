namespace SimpleAuth.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared;

    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject(IEnumerable<Claim> claims)
        {
            return Task.FromResult(Id.Create());
        }
    }
}