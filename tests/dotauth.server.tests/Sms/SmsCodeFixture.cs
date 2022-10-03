namespace DotAuth.Server.Tests.Sms;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using Moq;
using Xunit;
using Xunit.Abstractions;

public sealed class SmsCodeFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private readonly TestOauthServerFixture _server;

    public SmsCodeFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestOauthServerFixture(outputHelper);
    }

    [Fact]
    public async Task WhenNoPhoneNumberConfiguredThenReturnsError()
    {
        var client = CreateTokenClient();
        var noPhoneNumberResult = await client.RequestSms(new ConfirmationCodeRequest { PhoneNumber = string.Empty })
            .ConfigureAwait(false) as Option.Error;

        // ASSERT : NO PHONE NUMBER
        Assert.Equal(HttpStatusCode.BadRequest, noPhoneNumberResult.Details.Status);
        Assert.Equal(ErrorCodes.InvalidRequest, noPhoneNumberResult.Details.Title);
        Assert.Equal("parameter phone_number is missing", noPhoneNumberResult.Details.Detail);
    }

    private TokenClient CreateTokenClient()
    {
        return new(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + "/.well-known/openid-configuration"));
    }

    [Fact]
    public async Task WhenTwilioNotConfiguredThenReturnsError()
    {
        // ACT : TWILIO NO CONFIGURED
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult((ConfirmationCode)null));
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(true));
        _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, ""));
        var client = CreateTokenClient();
        var twilioNotConfigured = await client.RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, twilioNotConfigured.Details.Title);
        Assert.Equal("The SMS account is not properly configured", twilioNotConfigured.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, twilioNotConfigured.Details.Status);
    }

    [Fact]
    public async Task WhenNoConfirmationCodeThenReturnsError()
    {
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult((ConfirmationCode)null));
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(false));
        _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => { })
            .ReturnsAsync((true, null));
        var client = CreateTokenClient();
        var cannotInsertConfirmationCode = await client
            .RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            .ConfigureAwait(false) as Option.Error;

        // ASSERT : CANNOT INSERT CONFIRMATION CODE
        Assert.Equal(ErrorCodes.UnhandledExceptionCode, cannotInsertConfirmationCode.Details.Title);
        Assert.Equal("The confirmation code cannot be saved", cannotInsertConfirmationCode.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, cannotInsertConfirmationCode.Details.Status);
    }

    [Fact]
    public async Task WhenUnhandledExceptionOccursThenReturnsError()
    {
        _server.SharedCtx.TwilioClient.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, ""));
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult((ConfirmationCode)null));
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            .Callback(() => throw new Exception())
            .Returns(() => Task.FromResult(false));
        var client = CreateTokenClient();
        var unhandledException = await client.RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, unhandledException.Details!.Title);
        Assert.Equal(
            "unhandled exception occurred please contact the administrator",
            unhandledException.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, unhandledException.Details.Status);
    }

    [Fact]
    public async Task When_Send_ConfirmationCode_Then_Json_Is_Returned()
    {
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult((ConfirmationCode)null));
        _server.SharedCtx.ConfirmationCodeStore
            .Setup(h => h.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            //.Callback<ConfirmationCode>(r => { confirmationCode = r; })
            .Returns(() => Task.FromResult(true));
        _server.SharedCtx.TwilioClient.Setup(h => h.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => { })
            .ReturnsAsync((true, null));
        var client = CreateTokenClient();
        var happyPath = await client.RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            .ConfigureAwait(false);

        Assert.IsType<Option.Success>(happyPath);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server?.Dispose();
    }
}