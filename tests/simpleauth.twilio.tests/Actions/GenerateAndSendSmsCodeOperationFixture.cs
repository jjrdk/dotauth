namespace SimpleAuth.Twilio.Tests.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Moq;
    using SimpleAuth;
    using Twilio;
    using Twilio.Actions;
    using Xunit;

    public class GenerateAndSendSmsCodeOperationFixture
    {
        private const string _message = "Message {0}";
        private Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private SmsAuthenticationOptions _smsAuthenticationOptions;
        private Mock<ITwilioClient> _twilioClientStub;
        private IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

        [Fact]
        public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = await Assert
                .ThrowsAsync<ArgumentNullException>(() => _generateAndSendSmsCodeOperation.Execute(null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task When_TwilioSendException_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            _twilioClientStub.Setup(s =>
                    s.SendMessage(It.IsAny<TwilioSmsCredentials>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => throw new TwilioException("problem"));

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
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>()))
                .Returns(() => Task.FromResult(false));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _generateAndSendSmsCodeOperation.Execute("phoneNumber"))
                .ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Code);
            Assert.Equal("the confirmation code cannot be saved", exception.Message);
        }

        [Fact]
        public async Task When_GenerateAndSendConfirmationCode_Then_Code_Is_Returned()
        {
            InitializeFakeObjects();
            _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>()))
                .Returns(() => Task.FromResult(true));

            var confirmationCode = await _generateAndSendSmsCodeOperation.Execute("phoneNumber").ConfigureAwait(false);

            Assert.NotNull(confirmationCode);
        }

        private void InitializeFakeObjects()
        {
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _smsAuthenticationOptions = new SmsAuthenticationOptions
            {
                Message = _message
            };
            _twilioClientStub = new Mock<ITwilioClient>();
            _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
                _confirmationCodeStoreStub.Object,
                _smsAuthenticationOptions,
                _twilioClientStub.Object);
        }
    }
}
