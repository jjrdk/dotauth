namespace SimpleAuth.Tests.WebSite.User;

using Moq;
using Shared.Models;
using Shared.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using SimpleAuth.Properties;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.WebSite.User;
using Xunit;
using Xunit.Abstractions;

public sealed class UpdateUserClaimsOperationFixture
{
    private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
    private readonly UpdateUserClaimsOperation _updateUserClaimsOperation;

    public UpdateUserClaimsOperationFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
        _updateUserClaimsOperation = new UpdateUserClaimsOperation(
            _resourceOwnerRepositoryStub.Object,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
    {
        _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceOwner)null);

        var exception = await _updateUserClaimsOperation
            .Execute("subject", new List<Claim>(), CancellationToken.None)
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InternalError, exception!.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
    {
        _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ResourceOwner { Claims = new[] { new Claim("type", "value"), new Claim("type1", "value") } });

        await _updateUserClaimsOperation.Execute(
                "subjet",
                new List<Claim> { new("type", "value1") },
                CancellationToken.None)
            .ConfigureAwait(false);

        _resourceOwnerRepositoryStub.Verify(
            p => p.Update(
                It.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1")),
                It.IsAny<CancellationToken>()));
    }
}