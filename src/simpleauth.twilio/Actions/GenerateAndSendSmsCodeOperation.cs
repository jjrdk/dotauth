namespace SimpleAuth.Twilio.Actions
{
    using Errors;
    using Exceptions;
    using SimpleAuth;
    using System;
    using System.Threading.Tasks;

    internal sealed class GenerateAndSendSmsCodeOperation : IGenerateAndSendSmsCodeOperation
    {
        private readonly Random _random = new Random(DateTime.UtcNow.Second);
        private readonly IConfirmationCodeStore _confirmationCodeStore;
        private readonly SmsAuthenticationOptions _smsAuthenticationOptions;
        private readonly ITwilioClient _twilioClient;

        public GenerateAndSendSmsCodeOperation(
            IConfirmationCodeStore confirmationCodeStore,
            SmsAuthenticationOptions smsAuthenticationOptions,
            ITwilioClient twilioClient)
        {
            _confirmationCodeStore = confirmationCodeStore;
            _smsAuthenticationOptions = smsAuthenticationOptions;
            _twilioClient = twilioClient;
        }

        public async Task<string> Execute(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var confirmationCode = new ConfirmationCode
            {
                Value = await GetCode().ConfigureAwait(false),
                IssueAt = DateTime.UtcNow,
                ExpiresIn = 300,
                Subject = phoneNumber
            };

            var message = string.Format(_smsAuthenticationOptions.Message, confirmationCode.Value);
            try
            {
                await _twilioClient.SendMessage(_smsAuthenticationOptions.TwilioSmsCredentials, phoneNumber, message)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    "the twilio account is not properly configured",
                    ex);
            }

            if (!await _confirmationCodeStore.Add(confirmationCode).ConfigureAwait(false))
            {
                throw new SimpleAuthException(ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheConfirmationCodeCannotBeSaved);
            }

            return confirmationCode.Value;
        }

        private async Task<string> GetCode()
        {
            var number = _random.Next(100000, 999999);
            if (await _confirmationCodeStore.Get(number.ToString()).ConfigureAwait(false) != null)
            {
                return await GetCode().ConfigureAwait(false);
            }

            return number.ToString();
        }
    }
}
