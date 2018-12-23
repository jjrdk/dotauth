namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetClaimsAction
    {
        Task<IEnumerable<ClaimAggregate>> Execute();
    }
}