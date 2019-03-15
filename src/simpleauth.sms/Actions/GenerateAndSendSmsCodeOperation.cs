namespace SimpleAuth.Sms.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;

    internal sealed class GenerateAndSendSmsCodeOperation
    {
        private readonly Random _random = new Random(DateTime.UtcNow.Second);
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly ISmsClient _smsClient;

        public GenerateAndSendSmsCodeOperation(
            ISmsClient smsClient,
            IConfirmationCodeStore confirmationCodeStore)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _smsClient = smsClient;
        }

        public async Task<string> Execute(string phoneNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(cancellationToken).ConfigureAwait(false),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 300,
                Subject = phoneNumber
            };

            var message = "The confirmation code is " + confirmationCode.Value;
            try
            {
                await _smsClient.SendMessage(phoneNumber, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    "The SMS account is not properly configured",
                    ex);
            }

            if (!await _confirmationCodeStore.Add(confirmationCode, cancellationToken).ConfigureAwait(false))
            {
                throw new SimpleAuthException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            return confirmationCode.Value;
        }

        private async Task<string> GetCode(CancellationToken cancellationToken)
        {
            var number = _random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString(), cancellationToken).ConfigureAwait(false) != null)
            {
                return await GetCode(cancellationToken).ConfigureAwait(false);
            }

            return number.ToString();
        }
    }
}
