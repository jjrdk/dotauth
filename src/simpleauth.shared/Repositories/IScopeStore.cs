namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Parameters;
    using Results;

    public interface IScopeStore
    {
        Task<SearchScopeResult> Search(SearchScopesParameter parameter);
        Task<Scope> Get(string name);
        Task<ICollection<Scope>> SearchByNames(IEnumerable<string> names);
        Task<ICollection<Scope>> GetAll();
    }
}