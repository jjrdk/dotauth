namespace SimpleAuth.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public interface IPolicyClient
    {
        Task<AddPolicyResult> Add(PostPolicy request, string url, string token);
        Task<AddPolicyResult> AddByResolution(PostPolicy request, string url, string token);
        Task<GetPolicyResult> Get(string id, string url, string token);
        Task<GetPolicyResult> GetByResolution(string id, string url, string token);
        Task<GetPoliciesResult> GetAll(string url, string token);
        Task<GetPoliciesResult> GetAllByResolution(string url, string token);
        Task<BaseResponse> Delete(string id, string url, string token);
        Task<BaseResponse> DeleteByResolution(string id, string url, string token);
        Task<BaseResponse> Update(PutPolicy request, string url, string token);
        Task<BaseResponse> UpdateByResolution(PutPolicy request, string url, string token);
        Task<BaseResponse> AddResource(string id, PostAddResourceSet request, string url, string token);
        Task<BaseResponse> AddResourceByResolution(string id, PostAddResourceSet request, string url, string token);
        Task<BaseResponse> DeleteResource(string id, string resourceId, string url, string token);
        Task<BaseResponse> DeleteResourceByResolution(string id, string resourceId, string url, string token);
        Task<SearchAuthPoliciesResult> ResolveSearch(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }
}