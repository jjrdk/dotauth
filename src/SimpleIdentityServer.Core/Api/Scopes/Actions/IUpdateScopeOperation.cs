namespace SimpleIdentityServer.Core.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IUpdateScopeOperation
    {
        Task<bool> Execute(Scope scope);
    }
}