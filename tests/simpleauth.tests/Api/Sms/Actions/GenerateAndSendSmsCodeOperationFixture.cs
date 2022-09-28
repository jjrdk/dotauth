﻿namespace SimpleAuth.Tests.Api.Sms.Actions;

using Moq;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.Sms;
using SimpleAuth.Sms.Actions;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using SimpleAuth.Shared.Models;
using Xunit;
using Xunit.Abstractions;

public sealed class GenerateAndSendSmsCodeOperationFixture
{
    private readonly Mock<ISmsClient> _twilioClientStub;
    private readonly Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
    private readonly GenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;

    public GenerateAndSendSmsCodeOperationFixture(ITestOutputHelper outputHelper)
    {
        _twilioClientStub = new Mock<ISmsClient>();
        _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
        _generateAndSendSmsCodeOperation = new GenerateAndSendSmsCodeOperation(
            _twilioClientStub.Object,
            _confirmationCodeStoreStub.Object,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_TwilioSendException_Then_Exception_Is_Thrown()
    {
        _twilioClientStub.Setup(s => s.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, ""));

        var exception = await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None)
            .ConfigureAwait(false) as Option<string>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal("The SMS account is not properly configured", exception.Details.Detail);
    }

    [Fact]
    public async Task When_CannotInsert_ConfirmationCode_Then_Exception_Is_Thrown()
    {
        _twilioClientStub.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, ""));
        _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(false));

        var exception = await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None)
            .ConfigureAwait(false) as Option<string>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal("The confirmation code cannot be saved", exception.Details.Detail);
    }

    [Fact]
    public async Task When_GenerateAndSendConfirmationCode_Then_Code_Is_Returned()
    {
        _twilioClientStub.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, ""));
        _confirmationCodeStoreStub.Setup(c => c.Add(It.IsAny<ConfirmationCode>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(true));

        var confirmationCode = await _generateAndSendSmsCodeOperation.Execute("phoneNumber", CancellationToken.None).ConfigureAwait(false);

        Assert.NotNull(confirmationCode);
    }
}