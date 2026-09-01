namespace DotAuth.Mcp.Tests.StepDefinitions;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Mcp.Tests.Support;
using dotauth.mcp.Tools;
using DotAuth.Shared.Models;
using NSubstitute;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private UserTools _userTools = null!;

    [Given(@"no user with subject ""(.+)"" exists")]
    public void GivenNoUserWithSubjectExists(string subject)
    {
        _fixture ??= new McpServerFixture();
        _fixture.UserStore.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ResourceOwner?)null);
        _userTools = new UserTools(_fixture.UserStore);
    }

    [Given(@"a user with subject ""(.+)"" and password ""(.+)"" exists")]
    public void GivenAUserWithSubjectAndPasswordExists(string subject, string password)
    {
        _fixture ??= new McpServerFixture();
        _fixture.UserStore.Get(subject, Arg.Any<CancellationToken>())
            .Returns(new ResourceOwner
            {
                Subject = subject,
                Password = password,
                Claims = [new Claim("sub", subject), new Claim("email", $"{subject}@example.com")]
            });
        _userTools = new UserTools(_fixture.UserStore);
    }

    [When(@"get_user is invoked with subject ""(.+)""")]
    public async Task WhenGetUserIsInvokedWithSubject(string subject)
    {
        _userTools ??= new UserTools(_fixture.UserStore);
        _toolResult = await _userTools.GetUser(subject, CancellationToken.None);
    }

    [When(@"list_users is invoked")]
    public async Task WhenListUsersIsInvoked()
    {
        _fixture ??= new McpServerFixture();
        _userTools ??= new UserTools(_fixture.UserStore);
        _toolResult = await _userTools.ListUsers(CancellationToken.None);
    }

    [Then(@"the result indicates the user was not found")]
    public void ThenTheResultIndicatesTheUserWasNotFound()
    {
        Assert.NotNull(_toolResult);
        Assert.Contains("not found", _toolResult, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"the result does not contain ""(.+)""")]
    public void ThenTheResultDoesNotContain(string value)
    {
        Assert.NotNull(_toolResult);
        Assert.DoesNotContain(value, _toolResult, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"the result indicates listing is not supported")]
    public void ThenTheResultIndicatesListingIsNotSupported()
    {
        Assert.NotNull(_toolResult);
        Assert.Contains("not supported", _toolResult, StringComparison.OrdinalIgnoreCase);
    }
}
