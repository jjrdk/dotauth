namespace SimpleIdentityServer.Uma.Core.Api.PolicyController.Actions
{
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface ISearchAuthPoliciesAction
    {
        Task<SearchAuthPoliciesResult> Execute(SearchAuthPoliciesParameter parameter);
    }
}