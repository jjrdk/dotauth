namespace SimpleAuth.WebSite.User.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal sealed class UpdateUserCredentialsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserCredentialsOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string newPassword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.Password = newPassword;
            return await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        }
    }
}
