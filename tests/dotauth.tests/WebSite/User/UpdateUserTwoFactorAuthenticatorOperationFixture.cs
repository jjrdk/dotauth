namespace DotAuth.Tests.WebSite.User;

using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.User;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class UpdateUserTwoFactorAuthenticatorOperationFixture
{
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub;
    private readonly UpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

    public UpdateUserTwoFactorAuthenticatorOperationFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = Substitute.For<IResourceOwnerRepository>();
        _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
            _resourceOwnerRepositoryStub,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_ResourceOwner_Does_not_Exist_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ResourceOwner)null);

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
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceOwner());

        await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor", CancellationToken.None)
            .ConfigureAwait(false);

        await _resourceOwnerRepositoryStub.Received().Update(Arg.Any<ResourceOwner>(), Arg.Any<CancellationToken>());
    }
}
