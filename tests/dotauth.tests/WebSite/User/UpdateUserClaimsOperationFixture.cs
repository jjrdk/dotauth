namespace DotAuth.Tests.WebSite.User;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

public sealed class UpdateUserClaimsOperationFixture
{
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub;
    private readonly UpdateUserClaimsOperation _updateUserClaimsOperation;

    public UpdateUserClaimsOperationFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = Substitute.For<IResourceOwnerRepository>();
        _updateUserClaimsOperation = new UpdateUserClaimsOperation(
            _resourceOwnerRepositoryStub,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ResourceOwner)null);

        var exception = await _updateUserClaimsOperation
            .Execute("subject", new List<Claim>(), CancellationToken.None)
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InternalError, exception!.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
    {
        _resourceOwnerRepositoryStub.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new ResourceOwner { Claims = new[] { new Claim("type", "value"), new Claim("type1", "value") } });

        await _updateUserClaimsOperation.Execute(
                "subjet",
                new List<Claim> { new("type", "value1") },
                CancellationToken.None)
            .ConfigureAwait(false);

        await _resourceOwnerRepositoryStub.Received().Update(
            Arg.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1")),
            Arg.Any<CancellationToken>());
    }
}
