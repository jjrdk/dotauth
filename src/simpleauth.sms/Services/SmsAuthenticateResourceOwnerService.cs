namespace SimpleAuth.Sms.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class SmsAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public SmsAuthenticateResourceOwnerService(
            IResourceOwnerRepository resourceOwnerRepository,
            IConfirmationCodeStore confirmationCodeStore)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeStore = confirmationCodeStore;
        }

        public string Amr => SmsConstants.Amr;

        public async Task<ResourceOwner> AuthenticateResourceOwner(
            string login,
            string password,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var confirmationCode = await _confirmationCodeStore.Get(password).ConfigureAwait(false);
            if (confirmationCode == null || confirmationCode.Subject != login)
            {
                return null;
            }

            if (confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn) <= DateTime.UtcNow)
            {
                return null;
            }

            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(
                    OpenIdClaimTypes.PhoneNumber,
                    login,
                    cancellationToken)
                .ConfigureAwait(false);
            if (resourceOwner != null)
            {
                await _confirmationCodeStore.Remove(password).ConfigureAwait(false);
            }

            return resourceOwner;
        }
    }
}
