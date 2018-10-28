namespace SimpleIdentityServer.Core.Services
{
    using System;
    using System.Threading.Tasks;

    using Common.DTOs;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ISubjectBuilder
    {
        Task<string> BuildSubject(IList<Claim> claims, ScimUser scimUser = null);
    }

    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject(IList<Claim> claims, ScimUser scimUser = null)
        {
            return Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}