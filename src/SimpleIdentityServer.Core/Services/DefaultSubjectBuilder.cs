namespace SimpleIdentityServer.Core.Services
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Shared.DTOs;

    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject(IList<Claim> claims, ScimUser scimUser = null)
        {
            return Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}