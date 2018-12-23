namespace SimpleIdentityServer.Core.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared.DTOs;

    public interface ISubjectBuilder
    {
        Task<string> BuildSubject(IList<Claim> claims, ScimUser scimUser = null);
    }
}