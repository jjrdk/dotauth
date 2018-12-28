namespace SimpleAuth.Uma.Client.ResourceSet
{
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public interface IResourceSetClient
    {
        Task<UpdateResourceSetResult> Update(PutResourceSet request, string url, string token);
        Task<UpdateResourceSetResult> UpdateByResolution(PutResourceSet request, string url, string token);
        Task<AddResourceSetResult> Add(PostResourceSet request, string url, string token);
        Task<AddResourceSetResult> AddByResolution(PostResourceSet request, string url, string token);
        Task<BaseResponse> Delete(string id, string url, string token);
        Task<BaseResponse> DeleteByResolution(string id, string url, string token);
        Task<GetResourcesResult> GetAll(string url, string token);
        Task<GetResourcesResult> GetAllByResolution(string url, string token);
        Task<GetResourceSetResult> Get(string id,  string url, string token);
        Task<GetResourceSetResult> GetByResolution(string id, string url, string token);
        Task<SearchResourceSetResult> ResolveSearch(string url, SearchResourceSet parameter, string authorizationHeaderValue = null);
    }
}