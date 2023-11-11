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
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        var noPhoneNumberResult = Assert.IsType<Option.Error>(await client
            .RequestSms(new ConfirmationCodeRequest { PhoneNumber = string.Empty })
            );

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
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ConfirmationCode?>(default));
        _server.SharedCtx.ConfirmationCodeStore
            .Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _server.SharedCtx.TwilioClient.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, ""));
        var client = CreateTokenClient();
        var twilioNotConfigured = Assert.IsType<Option.Error>(await client
            .RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, twilioNotConfigured.Details.Title);
        Assert.Equal("The SMS account is not properly configured", twilioNotConfigured.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, twilioNotConfigured.Details.Status);
    }

    [Fact]
    public async Task WhenNoConfirmationCodeThenReturnsError()
    {
        _server.SharedCtx.ConfirmationCodeStore
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ConfirmationCode?>(null));
        _server.SharedCtx.ConfirmationCodeStore
            .Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _server.SharedCtx.TwilioClient.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, null));
        var client = CreateTokenClient();
        var cannotInsertConfirmationCode = Assert.IsType<Option.Error>(await client
            .RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            );

        // ASSERT : CANNOT INSERT CONFIRMATION CODE
        Assert.Equal(ErrorCodes.UnhandledExceptionCode, cannotInsertConfirmationCode.Details.Title);
        Assert.Equal("The confirmation code cannot be saved", cannotInsertConfirmationCode.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, cannotInsertConfirmationCode.Details.Status);
    }

    [Fact]
    public async Task WhenUnhandledExceptionOccursThenReturnsError()
    {
        _server.SharedCtx.TwilioClient.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, ""));
        _server.SharedCtx.ConfirmationCodeStore
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ConfirmationCode?>(null));
        _server.SharedCtx.ConfirmationCodeStore
            .Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Throws(new Exception());
        var client = CreateTokenClient();
        var unhandledException = Assert.IsType<Option.Error>(await client
            .RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, unhandledException.Details.Title);
        Assert.Equal(
            "unhandled exception occurred please contact the administrator",
            unhandledException.Details.Detail);
        Assert.Equal(HttpStatusCode.InternalServerError, unhandledException.Details.Status);
    }

    [Fact]
    public async Task When_Send_ConfirmationCode_Then_Json_Is_Returned()
    {
        _server.SharedCtx.ConfirmationCodeStore
            .Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ConfirmationCode?>(null));
        _server.SharedCtx.ConfirmationCodeStore
            .Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _server.SharedCtx.TwilioClient.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, null));
        var client = CreateTokenClient();
        var happyPath = await client.RequestSms(new ConfirmationCodeRequest { PhoneNumber = "phone" })
            ;

        Assert.IsType<Option.Success>(happyPath);
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
