namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface IProfileRepository
    {
        Task<ResourceOwnerProfile> Get(string subject);
        Task<bool> Add(params ResourceOwnerProfile[] profiles);
        Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter);
        Task<bool> Remove(IEnumerable<string> subjects);
    }
}