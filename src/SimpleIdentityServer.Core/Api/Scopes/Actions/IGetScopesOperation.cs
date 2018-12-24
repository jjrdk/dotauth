namespace SimpleIdentityServer.Core.Api.Scopes.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IGetScopesOperation
    {
        Task<ICollection<Scope>> Execute();
    }
}