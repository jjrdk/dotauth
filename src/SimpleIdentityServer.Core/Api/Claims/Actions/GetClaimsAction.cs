namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class GetClaimsAction : IGetClaimsAction
    {
        private readonly IClaimRepository _claimRepository;

        public GetClaimsAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }
        
        public Task<IEnumerable<ClaimAggregate>> Execute()
        {
            return _claimRepository.GetAllAsync();
        }
    }
}
