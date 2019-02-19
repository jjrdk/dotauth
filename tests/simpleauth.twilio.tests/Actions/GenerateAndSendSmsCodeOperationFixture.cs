namespace SimpleAuth.Twilio.Tests.Actions
{
    using Moq;
    using SimpleAuth;
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using Twilio;
    using Twilio.Actions;
    using Xunit;

    public class GenerateAndSendSmsCodeOperationFixture
    {
        private readonly Mock<ITwilioClient> _twilioClientStub;
        private const string Message = "Message {0}";
        private readonly Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

        public GenerateAndSendSmsCodeOperationFixture()
        {
            _twilioClientStub = new Mock<ITwilioClient>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            var smsAuthenticationOptions = new SmsAuthenticationOptions { Message = Message };
            _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
                _twilioClientStub.Object,
                _confirmationCodeStoreStub.Object,
                smsAuthenticationOptions);
        }

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            var exception = await Assert
                .ThrowsAsync<ArgumentNullException>(() => _generateAndSendSmsCodeOperation.Execute(null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_TwilioSendException_Then_Exception_Is_Thrown()
        {
            _twilioClientStub.Setup(s =>
                    s.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => throw new TwilioException("problem", null));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _generateAndSendSmsCodeOperation.Execute("phoneNumber"))
                .ConfigureAwait(false);

            //_eventSourceStub.Verify(e => e.Failure(It.Is<Exception>((f) => f.Message == "problem")));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal("the twilio account is not properly configured", exception.Message);
        }

        [Fact]
        public async Task When_CannotInsert_ConfirmationCode_Then_Exception_Is_Thrown()
        {
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>()))
                .Returns(() => Task.FromResult(false));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _generateAndSendSmsCodeOperation.Execute("phoneNumber"))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal("the confirmation code cannot be saved", exception.Message);
        }

        [Fact]
        public async Task When_GenerateAndSendConfirmationCode_Then_Code_Is_Returned()
        {
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>()))
                .Returns(() => Task.FromResult(true));

            var confirmationCode = await _generateAndSendSmsCodeOperation.Execute("phoneNumber").ConfigureAwait(false);

            Assert.NotNull(confirmationCode);
        }
    }
}
