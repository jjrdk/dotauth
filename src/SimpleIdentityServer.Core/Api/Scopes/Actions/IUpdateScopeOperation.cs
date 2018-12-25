namespace SimpleAuth.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IUpdateScopeOperation
    {
        Task<bool> Execute(Scope scope);
    }
}