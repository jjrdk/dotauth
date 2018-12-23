namespace SimpleIdentityServer.Core.Api.Scopes
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Results;

    public interface IScopeActions
    {
        Task<bool> DeleteScope(string scopeName);
        Task<Scope> GetScope(string scopeName);
        Task<ICollection<Scope>> GetScopes();
        Task<bool> AddScope(Scope scope);
        Task<bool> UpdateScope(Scope scope);
        Task<SearchScopeResult> Search(SearchScopesParameter parameter);
    }
}