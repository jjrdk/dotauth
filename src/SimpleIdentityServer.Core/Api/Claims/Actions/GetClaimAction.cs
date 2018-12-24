namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class GetClaimAction : IGetClaimAction
    {
        private readonly IClaimRepository _claimRepository;

        public GetClaimAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public async Task<ClaimAggregate> Execute(string claimCode)
        {
            if (string.IsNullOrWhiteSpace(claimCode))
            {
                throw new ArgumentNullException(nameof(claimCode));
            }

            var claim = await _claimRepository.GetAsync(claimCode).ConfigureAwait(false);
            if (claim == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, ErrorDescriptions.ClaimDoesntExist);
            }

            return await _claimRepository.GetAsync(claimCode).ConfigureAwait(false);
        }
    }
}
