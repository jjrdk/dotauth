using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims.Actions
{
    public interface IGetClaimsAction
    {
        Task<IEnumerable<ClaimAggregate>> Execute();
    }

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
