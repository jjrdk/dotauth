namespace SimpleAuth.Sms.Actions
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Sms.Properties;

    internal sealed class GenerateAndSendSmsCodeOperation
    {
        private readonly Random _random = new(DateTimeOffset.UtcNow.Second);
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly ILogger _logger;
        private readonly ISmsClient _smsClient;

        public GenerateAndSendSmsCodeOperation(
            ISmsClient smsClient,
            IConfirmationCodeStore confirmationCodeStore,
            ILogger logger)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _logger = logger;
            _smsClient = smsClient;
        }

        public async Task<Option<string>> Execute(string phoneNumber, CancellationToken cancellationToken)
        {
            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode(phoneNumber, cancellationToken).ConfigureAwait(false),
                IssueAt = DateTimeOffset.UtcNow,
                ExpiresIn = 120,
                Subject = phoneNumber
            };

            var message = string.Format(SmsStrings.TheConfirmationCodeIs, confirmationCode.Value);
            var sendResult = await _smsClient.SendMessage(phoneNumber, message).ConfigureAwait(false);

            if (!sendResult.Item1)
            {
                _logger.LogError(SmsStrings.TheSmsAccountIsNotProperlyConfigured);
                return new Option<string>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.UnhandledExceptionCode,
                        Detail = SmsStrings.TheSmsAccountIsNotProperlyConfigured,
                        Status = HttpStatusCode.InternalServerError
                    });
            }

            if (await _confirmationCodeStore.Add(confirmationCode, cancellationToken).ConfigureAwait(false))
            {
                return new Option<string>.Result(confirmationCode.Value);
            }

            _logger.LogError(SmsStrings.TheConfirmationCodeCannotBeSaved);
            return new Option<string>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = SmsStrings.TheConfirmationCodeCannotBeSaved,
                    Status = HttpStatusCode.InternalServerError
                });

        }

        private async Task<string> GetCode(string subject, CancellationToken cancellationToken)
        {
            var number = _random.Next(100000, 999999).ToString();
            if ((await _confirmationCodeStore.Get(number, subject, cancellationToken).ConfigureAwait(false))?.Value == number)
            {
                return await GetCode(subject, cancellationToken).ConfigureAwait(false);
            }

            return number;
        }
    }
}
