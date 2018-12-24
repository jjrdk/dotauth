namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Results;

    internal sealed class SearchClaimsAction : ISearchClaimsAction
    {
        private readonly IClaimRepository _claimRepository;

        public SearchClaimsAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public Task<SearchClaimsResult> Execute(SearchClaimsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _claimRepository.Search(parameter);
        }
    }
}
