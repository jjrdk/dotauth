namespace SimpleAuth.WebSite.User.Actions
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using Helpers;
    using Shared.Repositories;

    internal sealed class UpdateUserCredentialsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserCredentialsOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.Password = newPassword.ToSha256Hash();
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
        }
    }
}
