namespace SimpleIdentityServer.Uma.Client.Policy
{
    using System.Threading.Tasks;
    using Common.DTOs;
    using Results;

    public interface ISearchPoliciesOperation
    {
        Task<SearchAuthPoliciesResult> ExecuteAsync(string url, SearchAuthPolicies parameter, string authorizationHeaderValue = null);
    }
}