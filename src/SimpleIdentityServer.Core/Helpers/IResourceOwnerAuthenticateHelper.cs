namespace SimpleIdentityServer.Core.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IResourceOwnerAuthenticateHelper
    {
        Task<ResourceOwner> Authenticate(string login, string password, IEnumerable<string> exceptedAmrValues);
        IEnumerable<string> GetAmrs();
    }
}