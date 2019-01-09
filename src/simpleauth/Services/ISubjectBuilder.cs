namespace SimpleAuth.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared.DTOs;

    public interface ISubjectBuilder
    {
        Task<string> BuildSubject(IEnumerable<Claim> claims, ScimUser scimUser = null);
    }
}