// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Tests.WebSite.User;

using Moq;
using Shared;
using Shared.Models;
using Shared.Repositories;
using SimpleAuth.Shared.Errors;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using SimpleAuth.Properties;
using SimpleAuth.WebSite.User;
using Xunit;
using Xunit.Abstractions;

public sealed class GetUserOperationFixture
{
    private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
    private readonly GetUserOperation _getUserOperation;

    public GetUserOperationFixture(ITestOutputHelper outputHelper)
    {
        _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
        _getUserOperation = new GetUserOperation(
            _resourceOwnerRepositoryStub.Object,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_User_Is_Not_Authenticated_Then_Exception_Is_Thrown()
    {
        var emptyClaimsPrincipal = new ClaimsPrincipal();

        var exception = await _getUserOperation.Execute(emptyClaimsPrincipal, CancellationToken.None)
            .ConfigureAwait(false) as Option<ResourceOwner>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception.Details.Title);
        Assert.Equal(Strings.TheUserNeedsToBeAuthenticated, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Subject_Is_Not_Passed_Then_Exception_Is_Thrown()
    {
        var claimsIdentity = new ClaimsIdentity("test");
        claimsIdentity.AddClaim(new Claim("test", "test"));
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var exception = await _getUserOperation.Execute(claimsPrincipal, CancellationToken.None)
            .ConfigureAwait(false) as Option<ResourceOwner>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, exception!.Details.Title);
        Assert.Equal(Strings.TheSubjectCannotBeRetrieved, exception.Details.Detail);
    }

    [Fact]
    public void When_Correct_Subject_Is_Passed_Then_ResourceOwner_Is_Returned()
    {
        var claimsIdentity = new ClaimsIdentity("test");
        claimsIdentity.AddClaim(new Claim(OpenIdClaimTypes.Subject, "subject"));
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResourceOwner());

        var result = _getUserOperation.Execute(claimsPrincipal, CancellationToken.None);

        Assert.NotNull(result);
    }
}