namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Shared.Repositories;

    public interface IDeleteClaimAction
    {
        Task<bool> Execute(string claimCode);
    }

    internal sealed class DeleteClaimAction : IDeleteClaimAction
    {
        private readonly IClaimRepository _claimRepository;

        public DeleteClaimAction(IClaimRepository claimRepository)
        {
            _claimRepository = claimRepository;
        }

        public async Task<bool> Execute(string claimCode)
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

            if (claim.IsIdentifier)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, ErrorDescriptions.CannotRemoveClaimIdentifier);
            }

            return await _claimRepository.Delete(claimCode).ConfigureAwait(false);
        }
    }
}
