namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Results;
    using SimpleAuth.Uma.Shared.DTOs;

    public interface ISearchPoliciesOperation
    {
        Task<SearchAuthPoliciesResult> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }
}