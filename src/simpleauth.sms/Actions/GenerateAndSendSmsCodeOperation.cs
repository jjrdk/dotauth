namespace SimpleAuth.Sms.Actions
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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
            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(cancellationToken).ConfigureAwait(false),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 120,
                Subject = phoneNumber
            };

            var message = "The confirmation code is " + confirmationCode.Value;
            var sendResult = await _smsClient.SendMessage(phoneNumber, message).ConfigureAwait(false);

            if (!sendResult.Item1)
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    "The SMS account is not properly configured");
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
            var number = _random.Next(100000, 999999).ToString();
            if ((await _confirmationCodeStore.Get(number, cancellationToken).ConfigureAwait(false))?.Value == number)
            {
                return await GetCode(cancellationToken).ConfigureAwait(false);
            }

            return number;
        }
    }
}
