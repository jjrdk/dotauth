namespace SimpleIdentityServer.Uma.Core.Api.PolicyController
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface IPolicyActions
    {
        Task<string> AddPolicy(AddPolicyParameter addPolicyParameter);
        Task<Policy> GetPolicy(string policyId);
        Task<bool> DeletePolicy(string policyId);
        Task<bool> UpdatePolicy(UpdatePolicyParameter updatePolicyParameter);
        Task<ICollection<string>> GetPolicies();
        Task<bool> AddResourceSet(AddResourceSetParameter addResourceSetParameter);
        Task<bool> DeleteResourceSet(string id, string resourceId);
        Task<SearchAuthPoliciesResult> Search(SearchAuthPoliciesParameter parameter);
    }
}