namespace SimpleAuth.Tests.Api.Sms.Actions;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using Moq;
using SimpleAuth.Events;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.Sms.Actions;
using Xunit;
using Xunit.Abstractions;

public sealed class SmsAuthenticationOperationFixture
{
    private readonly SmsAuthenticationOperation _smsAuthenticationOperation;

    public SmsAuthenticationOperationFixture(ITestOutputHelper outputHelper)
    {
        var generateAndSendSmsCodeOperationStub = new Mock<IConfirmationCodeStore>();
        var resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
        var subjectBuilderStub = new Mock<ISubjectBuilder>();
        subjectBuilderStub.Setup(x => x.BuildSubject(It.IsAny<IEnumerable<Claim>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow.Ticks.ToString);
        _smsAuthenticationOperation = new SmsAuthenticationOperation(
            new RuntimeSettings(),
            null,
            generateAndSendSmsCodeOperationStub.Object,
            resourceOwnerRepositoryStub.Object,
            subjectBuilderStub.Object,
            Array.Empty<IAccountFilter>(),
            new Mock<IEventPublisher>().Object,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_Null_Parameter_Is_Passed_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _smsAuthenticationOperation.Execute(null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Empty_Parameter_Is_Passed_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _smsAuthenticationOperation.Execute(string.Empty, CancellationToken.None))
            .ConfigureAwait(false);
    }
}