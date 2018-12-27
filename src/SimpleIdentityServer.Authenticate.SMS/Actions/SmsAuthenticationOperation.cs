namespace SimpleAuth.Authenticate.Twilio.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Helpers;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Services;
    using WebSite.User.Actions;

    internal sealed class SmsAuthenticationOperation : ISmsAuthenticationOperation
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAddUserOperation _userActions;
        private readonly SmsAuthenticationOptions _smsAuthenticationOptions;
        private readonly ISubjectBuilder _subjectBuilder;

        public SmsAuthenticationOperation(
            IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation,
            IResourceOwnerRepository resourceOwnerRepository,
            IAddUserOperation userActions,
            ISubjectBuilder subjectBuilder,
            SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _resourceOwnerRepository = resourceOwnerRepository;
            _userActions = userActions;
            _subjectBuilder = subjectBuilder;
            _smsAuthenticationOptions = smsAuthenticationOptions;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            // 1. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber).ConfigureAwait(false);
            // 2. Try to get the resource owner.
            var resourceOwner = await _resourceOwnerRepository
                .GetResourceOwnerByClaim(JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber)
                .ConfigureAwait(false);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            // 3. Create a new resource owner.
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber),
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false")
            };
            var id = await _subjectBuilder.BuildSubject(claims).ConfigureAwait(false);
            var record = new ResourceOwner
            {
                Id = id,
                Password = Guid.NewGuid().ToString("N").ToSha256Hash(),
                Claims = claims
            };
            // 3.1 Add scim resource.
            if (_smsAuthenticationOptions.ScimBaseUrl != null)
            {
                await _userActions.Execute(
                        record,
                        _smsAuthenticationOptions.ScimBaseUrl)
                    .ConfigureAwait(false);
            }
            else
            {
                // 3.2 Add user.
                await _userActions.Execute(record).ConfigureAwait(false);
            }

            return await _resourceOwnerRepository
                .GetResourceOwnerByClaim(JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber)
                .ConfigureAwait(false);
        }
    }
}
