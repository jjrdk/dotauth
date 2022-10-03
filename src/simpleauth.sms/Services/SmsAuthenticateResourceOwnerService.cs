namespace DotAuth.Sms.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

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

    public string Amr
    {
        get { return SmsConstants.Amr; }
    }

    public async Task<ResourceOwner?> AuthenticateResourceOwner(
        string login,
        string password,
        CancellationToken cancellationToken)
    {
        var confirmationCode =
            await _confirmationCodeStore.Get(password, login, cancellationToken).ConfigureAwait(false);
        if (confirmationCode == null || confirmationCode.Subject != login)
        {
            return null;
        }

        if (confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn) <= DateTimeOffset.UtcNow)
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
            await _confirmationCodeStore.Remove(password, resourceOwner.Subject!, cancellationToken)
                .ConfigureAwait(false);
        }

        return resourceOwner;
    }
}