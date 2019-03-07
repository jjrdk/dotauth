namespace SimpleAuth.Server.Tests.Sms
{
    using Moq;
    using SimpleAuth;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using SimpleAuth.Sms.Client;
    using Xunit;

    public class SmsCodeFixture : IDisposable
    {
        private readonly SidSmsAuthenticateClient _sidSmsAuthenticateClient;
        private const string BaseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public SmsCodeFixture()
        {
            _server = new TestOauthServerFixture();
            _sidSmsAuthenticateClient = new SidSmsAuthenticateClient(_server.Client);
        }

        [Fact]
        public async Task WhenNoPhoneNumberConfiguredThenReturnsError()
        {
            // ACT : NO PHONE NUMBER
            var noPhoneNumberResult = await _sidSmsAuthenticateClient
                .Send(BaseUrl, new ConfirmationCodeRequest { PhoneNumber = string.Empty })
                .ConfigureAwait(false);

            // ASSERT : NO PHONE NUMBER
            Assert.True(noPhoneNumberResult.ContainsError);
            Assert.Equal(HttpStatusCode.BadRequest, noPhoneNumberResult.HttpStatus);
            Assert.Equal(ErrorCodes.InvalidRequestCode, noPhoneNumberResult.Error.Error);
            Assert.Equal("parameter phone_number is missing", noPhoneNumberResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task WhenTwilioNotConfiguredThenReturnsError()
        {
            // ACT : TWILIO NO CONFIGURED
            ConfirmationCode confirmationCode;
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ConfirmationCode)null));
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>()))
                .Callback<ConfirmationCode>(r => { confirmationCode = r; })
                .Returns(() => Task.FromResult(true));
            _server.SharedCtx.TwilioClient
                .Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(
                    () => throw new SimpleAuthException(
                        ErrorCodes.UnhandledExceptionCode,
                        "the twilio account is not properly configured"));
            var twilioNotConfigured = await _sidSmsAuthenticateClient
                .Send(BaseUrl, new ConfirmationCodeRequest { PhoneNumber = "phone" })
                .ConfigureAwait(false);

            Assert.True(twilioNotConfigured.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, twilioNotConfigured.Error.Error);
            Assert.Equal("The SMS account is not properly configured", twilioNotConfigured.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, twilioNotConfigured.HttpStatus);
        }

        [Fact]
        public async Task WhenNoConfirmationCodeThenReturnsError()
        {
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ConfirmationCode)null));
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>()))
                // .Callback<ConfirmationCode>(r => { confirmationCode = r; })
                .Returns(() => Task.FromResult(false));
            _server.SharedCtx.TwilioClient
                .Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .ReturnsAsync(true);
            var cannotInsertConfirmationCode = await _sidSmsAuthenticateClient
                .Send(BaseUrl, new ConfirmationCodeRequest { PhoneNumber = "phone" })
                .ConfigureAwait(false);

            // ASSERT : CANNOT INSERT CONFIRMATION CODE
            Assert.True(cannotInsertConfirmationCode.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, cannotInsertConfirmationCode.Error.Error);
            Assert.Equal("the confirmation code cannot be saved", cannotInsertConfirmationCode.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, cannotInsertConfirmationCode.HttpStatus);
        }

        [Fact]
        public async Task WhenUnhandledExceptionOccursThenReturnsError()
        {
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ConfirmationCode)null));
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>()))
                .Callback(() => throw new Exception())
                .Returns(() => Task.FromResult(false));
            var unhandledException = await _sidSmsAuthenticateClient
                .Send(BaseUrl, new ConfirmationCodeRequest { PhoneNumber = "phone" })
                .ConfigureAwait(false);

            Assert.True(unhandledException.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, unhandledException.Error.Error);
            Assert.Equal(
                "unhandled exception occured please contact the administrator",
                unhandledException.Error.ErrorDescription);
            Assert.Equal(HttpStatusCode.InternalServerError, unhandledException.HttpStatus);
        }

        [Fact]
        public async Task When_Send_ConfirmationCode_Then_Json_Is_Returned()
        {
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ConfirmationCode)null));
            _server.SharedCtx.ConfirmationCodeStore.Setup(h => h.Add(It.IsAny<ConfirmationCode>()))
                //.Callback<ConfirmationCode>(r => { confirmationCode = r; })
                .Returns(() => Task.FromResult(true));
            _server.SharedCtx.TwilioClient
                .Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .ReturnsAsync(true);
            var happyPath = await _sidSmsAuthenticateClient
                .Send(BaseUrl, new ConfirmationCodeRequest { PhoneNumber = "phone" })
                .ConfigureAwait(false);

            Assert.False(happyPath.ContainsError);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
