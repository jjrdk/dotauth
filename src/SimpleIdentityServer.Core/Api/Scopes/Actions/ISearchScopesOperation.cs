namespace SimpleAuth.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using Shared.Parameters;
    using Shared.Results;

    public interface ISearchScopesOperation
    {
        Task<SearchScopeResult> Execute(SearchScopesParameter parameter);
    }
}