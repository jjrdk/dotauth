namespace SimpleIdentityServer.Core.Api.Claims
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Results;

    public interface IClaimActions
    {
        Task<bool> Add(AddClaimParameter request);
        Task<bool> Delete(string claimCode);
        Task<ClaimAggregate> Get(string claimCode);
        Task<SearchClaimsResult> Search(SearchClaimsParameter parameter);
        Task<IEnumerable<ClaimAggregate>> GetAll();
    }
}