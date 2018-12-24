namespace SimpleIdentityServer.Core.Api.Profile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IProfileActions
    {
        Task<bool> Unlink(string localSubject, string externalSubject);
        Task<bool> Link(string localSubject, string externalSubject, string issuer, bool force = false);
        Task<IEnumerable<ResourceOwnerProfile>> GetProfiles(string subject);
        Task<ResourceOwner> GetResourceOwner(string externalSubject);
    }
}