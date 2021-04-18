namespace SimpleAuth.WebSite.User
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class UpdateUserTwoFactorAuthenticatorOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly ILogger _logger;

        public UpdateUserTwoFactorAuthenticatorOperation(IResourceOwnerRepository resourceOwnerRepository, ILogger logger)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _logger = logger;
        }

        public async Task<Option> Execute(string subject, string twoFactorAuth, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                _logger.LogError("Subject is null");
                throw new ArgumentNullException(nameof(subject));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                _logger.LogError(Strings.TheRoDoesntExist);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InternalError,
                        Detail = Strings.TheRoDoesntExist,
                        Status = HttpStatusCode.InternalServerError
                    });
            }

            resourceOwner.TwoFactorAuthentication = twoFactorAuth;
            return await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        }
    }
}
