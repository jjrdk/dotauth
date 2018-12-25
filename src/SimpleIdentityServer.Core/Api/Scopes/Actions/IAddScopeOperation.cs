namespace SimpleAuth.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IAddScopeOperation
    {
        Task<bool> Execute(Scope scope);
    }
}