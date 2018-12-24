namespace SimpleIdentityServer.Core.Api.Scopes.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IGetScopeOperation
    {
        Task<Scope> Execute(string scopeName);
    }
}