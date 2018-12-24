namespace SimpleIdentityServer.Core.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Results;

    public interface ISearchScopesOperation
    {
        Task<SearchScopeResult> Execute(SearchScopesParameter parameter);
    }
}