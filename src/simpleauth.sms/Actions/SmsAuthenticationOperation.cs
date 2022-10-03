namespace DotAuth.Sms.Actions;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Events;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.User;
using Microsoft.Extensions.Logging;

internal sealed class SmsAuthenticationOperation
{
    private readonly string _salt;
    private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
    private readonly IResourceOwnerRepository _resourceOwnerRepository;
    private readonly AddUserOperation _addUser;
    private readonly ISubjectBuilder _subjectBuilder;

    public SmsAuthenticationOperation(
        RuntimeSettings settings,
        ISmsClient smsClient,
        IConfirmationCodeStore confirmationCodeStore,
        IResourceOwnerRepository resourceOwnerRepository,
        ISubjectBuilder subjectBuilder,
        IAccountFilter[] accountFilters,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _salt = settings.Salt;
        _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
            smsClient,
            confirmationCodeStore,
            logger);
        _resourceOwnerRepository = resourceOwnerRepository;
        _addUser = new AddUserOperation(settings, resourceOwnerRepository, accountFilters, subjectBuilder, eventPublisher);
        _subjectBuilder = subjectBuilder;
    }

    public async Task<Option<ResourceOwner>> Execute(string phoneNumber, CancellationToken cancellationToken)
    {
        // 1. Send the confirmation code (SMS).
        var option = await _generateAndSendSmsCodeOperation.Execute(phoneNumber, cancellationToken).ConfigureAwait(false);
        if (option is Option<string>.Error e)
        {
            return new Option<ResourceOwner>.Error(e.Details, e.State);
        }

        // 2. Try to get the resource owner.
        var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(
                OpenIdClaimTypes.PhoneNumber,
                phoneNumber,
                cancellationToken)
            .ConfigureAwait(false);
        if (resourceOwner != null)
        {
            return new Option<ResourceOwner>.Result(resourceOwner);
        }

        // 3. CreateJwk a new resource owner.
        var claims = new[]
        {
            new Claim(OpenIdClaimTypes.PhoneNumber, phoneNumber),
            new Claim(OpenIdClaimTypes.PhoneNumberVerified, "false")
        };
        var id = await _subjectBuilder.BuildSubject(claims, cancellationToken).ConfigureAwait(false);
        var record = new ResourceOwner { Subject = id, Password = DotAuth.Id.Create().ToSha256Hash(_salt), Claims = claims };

        // 3.2 Add user.
        await _addUser.Execute(record, cancellationToken).ConfigureAwait(false);
        //}

        var result = await _resourceOwnerRepository.GetResourceOwnerByClaim(
                OpenIdClaimTypes.PhoneNumber,
                phoneNumber,
                cancellationToken)
            .ConfigureAwait(false);
        return new Option<ResourceOwner>.Result(result!);
    }
}