namespace SimpleAuth.Api.ResourceSetController
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IResourceSetActions
    {
        Task<string> AddResourceSet(AddResouceSetParameter addResouceSetParameter);
        Task<ResourceSet> GetResourceSet(string id);
        Task<bool> UpdateResourceSet(UpdateResourceSetParameter updateResourceSetParameter);
        Task<bool> RemoveResourceSet(string resourceSetId);
        Task<IEnumerable<string>> GetAllResourceSet();
        Task<IEnumerable<string>> GetPolicies(string resourceId);
        Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter);
    }
}