namespace SimpleAuth.Api.PolicyController.Actions
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface ISearchAuthPoliciesAction
    {
        Task<SearchAuthPoliciesResult> Execute(SearchAuthPoliciesParameter parameter);
    }
}