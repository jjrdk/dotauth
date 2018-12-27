namespace SimpleAuth.Twilio.Services
{
    using System;
    using System.Threading.Tasks;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Services;

    internal sealed class SmsAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public SmsAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository, IConfirmationCodeStore confirmationCodeStore)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _confirmationCodeStore = confirmationCodeStore;
        }

        public string Amr => SmsConstants.AMR;

        public async Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
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

            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber, login).ConfigureAwait(false);
            if (resourceOwner != null)
            {
                await _confirmationCodeStore.Remove(password).ConfigureAwait(false);
            }

            return resourceOwner;
        }
    }
}
