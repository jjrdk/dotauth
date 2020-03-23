namespace SimpleAuth.Server.Tests.Sms
{
    using Moq;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using Xunit;

    public class SmsCodeFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;

        public SmsCodeFixture()
        {
            _server = new TestOauthServerFixture();
        }

        [Fact]
        public async Task WhenNoPhoneNumberConfiguredThenReturnsError()
        {
            var client = CreateTokenClient();
            var noPhoneNumberResult = await client.RequestSms(new ConfirmationCodeRequest {PhoneNumber = string.Empty})
                .ConfigureAwait(false);

            // ASSERT : NO PHONE NUMBER
            Assert.True(noPhoneNumberResult.HasError);
            Assert.Equal(HttpStatusCode.BadRequest, noPhoneNumberResult.StatusCode);
            Assert.Equal(ErrorCodes.InvalidRequest, noPhoneNumberResult.Error.Title);
            Assert.Equal("parameter phone_number is missing", noPhoneNumberResult.Error.Detail);
        }

        private TokenClient CreateTokenClient()
        {
            return new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + "/.well-known/openid-configuration"));
        }

        [Fact]
        public async Task WhenTwilioNotConfiguredThenReturnsError()
        {
            // ACT : TWILIO NO CONFIGURED
            ConfirmationCode confirmationCode;
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((ConfirmationCode) null));
            _server.SharedCtx.ConfirmationCodeStore
                .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .Callback<ConfirmationCode, CancellationToken>((r, c) => { confirmationCode = r; })
                .Returns(() => Task.FromResult(true));
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, ""));
            var client = CreateTokenClient();
            var twilioNotConfigured = await client.RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
                .ConfigureAwait(false);

            Assert.True(twilioNotConfigured.HasError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, twilioNotConfigured.Error.Title);
            Assert.Equal("The SMS account is not properly configured", twilioNotConfigured.Error.Detail);
            Assert.Equal(HttpStatusCode.InternalServerError, twilioNotConfigured.StatusCode);
        }

        [Fact]
        public async Task WhenNoConfirmationCodeThenReturnsError()
        {
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((ConfirmationCode) null));
            _server.SharedCtx.ConfirmationCodeStore
                .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                // .Callback<ConfirmationCode>(r => { confirmationCode = r; })
                .Returns(() => Task.FromResult(false));
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .ReturnsAsync((true, null));
            var client = CreateTokenClient();
            var cannotInsertConfirmationCode = await client
                .RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
                .ConfigureAwait(false);

            // ASSERT : CANNOT INSERT CONFIRMATION CODE
            Assert.True(cannotInsertConfirmationCode.HasError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, cannotInsertConfirmationCode.Error.Title);
            Assert.Equal("the confirmation code cannot be saved", cannotInsertConfirmationCode.Error.Detail);
            Assert.Equal(HttpStatusCode.InternalServerError, cannotInsertConfirmationCode.StatusCode);
        }

        [Fact]
        public async Task WhenUnhandledExceptionOccursThenReturnsError()
        {
            _server.SharedCtx.TwilioClient.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, ""));
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((ConfirmationCode) null));
            _server.SharedCtx.ConfirmationCodeStore
                .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                .Callback(() => throw new Exception())
                .Returns(() => Task.FromResult(false));
            var client = CreateTokenClient();
            var unhandledException = await client.RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
                .ConfigureAwait(false);

            Assert.True(unhandledException.HasError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, unhandledException.Error.Title);
            Assert.Equal(
                "unhandled exception occured please contact the administrator",
                unhandledException.Error.Detail);
            Assert.Equal(HttpStatusCode.InternalServerError, unhandledException.StatusCode);
        }

        [Fact]
        public async Task When_Send_ConfirmationCode_Then_Json_Is_Returned()
        {
            _server.SharedCtx.ConfirmationCodeStore.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((ConfirmationCode) null));
            _server.SharedCtx.ConfirmationCodeStore
                .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
                //.Callback<ConfirmationCode>(r => { confirmationCode = r; })
                .Returns(() => Task.FromResult(true));
            _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .ReturnsAsync((true, null));
            var client = CreateTokenClient();
            var happyPath = await client.RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
                .ConfigureAwait(false);

            Assert.False(happyPath.HasError);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
