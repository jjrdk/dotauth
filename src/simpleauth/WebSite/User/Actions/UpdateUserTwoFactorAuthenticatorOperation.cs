namespace SimpleAuth.WebSite.User.Actions
{
    using Exceptions;
    using Shared.Repositories;
    using System;
    using System.Threading.Tasks;

    internal sealed class UpdateUserTwoFactorAuthenticatorOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserTwoFactorAuthenticatorOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string twoFactorAuth)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.TwoFactorAuthentication = twoFactorAuth;
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
        }
    }
}
