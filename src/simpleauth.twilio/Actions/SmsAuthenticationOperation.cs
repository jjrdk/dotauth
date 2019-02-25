namespace SimpleAuth.Twilio.Actions
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using WebSite.User.Actions;

    internal sealed class SmsAuthenticationOperation
    {
        private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly AddUserOperation _addUser;
        private readonly ISubjectBuilder _subjectBuilder;

        public SmsAuthenticationOperation(
            ITwilioClient twilioClient,
            IConfirmationCodeStore confirmationCodeStore,
            IResourceOwnerRepository resourceOwnerRepository,
            ISubjectBuilder subjectBuilder,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher,
            SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
                twilioClient,
                confirmationCodeStore,
                smsAuthenticationOptions);
            _resourceOwnerRepository = resourceOwnerRepository;
            _addUser = new AddUserOperation(resourceOwnerRepository, accountFilters, eventPublisher);
            _subjectBuilder = subjectBuilder;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            // 1. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber).ConfigureAwait(false);
            // 2. Try to get the resource owner.
            var resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(
                    OpenIdClaimTypes.PhoneNumber,
                    phoneNumber,
                    cancellationToken)
                .ConfigureAwait(false);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            // 3. CreateJwk a new resource owner.
            var claims = new[]
            {
                new Claim(OpenIdClaimTypes.PhoneNumber, phoneNumber),
                new Claim(OpenIdClaimTypes.PhoneNumberVerified, "false")
            };
            var id = await _subjectBuilder.BuildSubject(claims).ConfigureAwait(false);
            var record = new ResourceOwner {Subject = id, Password = Id.Create().ToSha256Hash(), Claims = claims};

            // 3.2 Add user.
            await _addUser.Execute(record, cancellationToken).ConfigureAwait(false);
            //}

            return await _resourceOwnerRepository.GetResourceOwnerByClaim(
                    OpenIdClaimTypes.PhoneNumber,
                    phoneNumber,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
