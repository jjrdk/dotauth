namespace SimpleAuth.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetScopeOperation
    {
        Task<Scope> Execute(string scopeName);
    }
}