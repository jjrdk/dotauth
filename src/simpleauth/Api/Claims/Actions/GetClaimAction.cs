namespace SimpleAuth.Api.Claims.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Shared.Models;
    using Shared.Repositories;

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
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, ErrorDescriptions.ClaimDoesntExist);
            }

            return await _claimRepository.GetAsync(claimCode).ConfigureAwait(false);
        }
    }
}
