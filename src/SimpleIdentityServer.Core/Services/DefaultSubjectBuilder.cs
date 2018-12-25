namespace SimpleAuth.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared.DTOs;

    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject(IList<Claim> claims, ScimUser scimUser = null)
        {
            return Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}