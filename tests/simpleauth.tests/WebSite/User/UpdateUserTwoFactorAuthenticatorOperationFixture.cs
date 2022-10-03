﻿namespace DotAuth.Tests.WebSite.User;

using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.User;
using Moq;
using Xunit;
using Xunit.Abstractions;

public sealed class UpdateUserTwoFactorAuthenticatorOperationFixture
{
    private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
    private readonly UpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

    public UpdateUserTwoFactorAuthenticatorOperationFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
        _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
            _resourceOwnerRepositoryStub.Object,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_ResourceOwner_Does_not_Exist_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceOwner)null);

        var exception = await _updateUserTwoFactorAuthenticatorOperation.Execute(
                "subject",
                "two_factor",
                CancellationToken.None)
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InternalError, exception!.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
    {
        _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResourceOwner());

        await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor", CancellationToken.None)
            .ConfigureAwait(false);

        _resourceOwnerRepositoryStub.Verify(
            r => r.Update(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()));
    }
}