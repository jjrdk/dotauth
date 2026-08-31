namespace DotAuth.Mcp.Tests.StepDefinitions;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Mcp.Tests.Support;
using DotAuth.Mcp.Tools;
using DotAuth.Shared.Models;
using NSubstitute;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private ScopeTools _scopeTools = null!;

    [Given(@"scopes ""(.+)"" and ""(.+)"" are registered")]
    public void GivenScopesAreRegistered(string name1, string name2)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ScopeStore.GetAll(Arg.Any<CancellationToken>())
            .Returns(
            [
                new Scope { Name = name1, Description = $"{name1} scope" },
                new Scope { Name = name2, Description = $"{name2} scope" }
            ]);
        _scopeTools = new ScopeTools(_fixture.ScopeStore);
    }

    [Given(@"no scope named ""(.+)"" is registered")]
    public void GivenNoScopeNamedIsRegistered(string name)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ScopeStore.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Scope?)null);
        _scopeTools = new ScopeTools(_fixture.ScopeStore);
    }

    [Given(@"a scope named ""(.+)"" with description ""(.+)"" is registered")]
    public void GivenAScopeIsRegistered(string name, string description)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ScopeStore.Get(name, Arg.Any<CancellationToken>())
            .Returns(new Scope { Name = name, Description = description });
        _scopeTools = new ScopeTools(_fixture.ScopeStore);
    }

    [When(@"list_scopes is invoked")]
    public async Task WhenListScopesIsInvoked()
    {
        _toolResult = await _scopeTools.ListScopes(CancellationToken.None);
    }

    [When(@"get_scope is invoked with name ""(.+)""")]
    public async Task WhenGetScopeIsInvokedWithName(string name)
    {
        _toolResult = await _scopeTools.GetScope(name, CancellationToken.None);
    }

    [Then(@"the result indicates the scope was not found")]
    public void ThenTheResultIndicatesTheScopeWasNotFound()
    {
        Assert.NotNull(_toolResult);
        Assert.Contains("not found", _toolResult, StringComparison.OrdinalIgnoreCase);
    }
}
