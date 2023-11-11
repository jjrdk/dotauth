namespace DotAuth.Tests.Api.Sms.Actions;

using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Sms;
using DotAuth.Sms.Actions;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class GenerateAndSendSmsCodeOperationFixture
{
    private readonly ISmsClient _twilioClientStub;
    private readonly IConfirmationCodeStore _confirmationCodeStoreStub;
    private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

    public GenerateAndSendSmsCodeOperationFixture(ITestOutputHelper outputHelper)
    {
        _twilioClientStub = Substitute.For<ISmsClient>();
        _confirmationCodeStoreStub = Substitute.For<IConfirmationCodeStore>();
        _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
            _twilioClientStub,
            _confirmationCodeStoreStub,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_TwilioSendException_Then_Exception_Is_Thrown()
    {
        _twilioClientStub.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, ""));

        var exception = Assert.IsType<Option<string>.Error>(
            await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception!.Details.Title);
        Assert.Equal("The SMS account is not properly configured", exception.Details.Detail);
    }

    [Fact]
    public async Task When_CannotInsert_ConfirmationCode_Then_Exception_Is_Thrown()
    {
        _twilioClientStub.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, ""));
        _confirmationCodeStoreStub.Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var exception = Assert.IsType<Option<string>.Error>(
            await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception!.Details.Title);
        Assert.Equal("The confirmation code cannot be saved", exception.Details.Detail);
    }

    [Fact]
    public async Task When_GenerateAndSendConfirmationCode_Then_Code_Is_Returned()
    {
        _twilioClientStub.SendMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, ""));
        _confirmationCodeStoreStub.Add(Arg.Any<ConfirmationCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var confirmationCode = await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None)
            ;

        Assert.NotNull(confirmationCode);
    }
}
