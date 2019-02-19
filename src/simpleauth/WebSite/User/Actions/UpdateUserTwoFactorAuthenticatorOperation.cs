namespace SimpleAuth.WebSite.User.Actions
{
    using Shared.Repositories;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal sealed class UpdateUserTwoFactorAuthenticatorOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserTwoFactorAuthenticatorOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string twoFactorAuth, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.TwoFactorAuthentication = twoFactorAuth;
            return await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        }
    }
}
