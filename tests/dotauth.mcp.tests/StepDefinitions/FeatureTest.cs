namespace DotAuth.Mcp.Tests.StepDefinitions;

using System;
using System.Net.Http;
using DotAuth.Mcp.Tests.Support;
using Reqnroll;
using Xunit;

/// <summary>
/// Base partial class for all MCP test scenarios.
/// Each partial file adds step definitions for one feature area.
/// </summary>
[Binding]
public partial class FeatureTest : IDisposable
{
    private readonly ITestOutputHelper _output;

    // Shared state across step definitions within a scenario.
    private McpServerFixture _fixture = null!;
    private HttpResponseMessage? _response;
    private string? _toolResult;

    public FeatureTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Given(@"a running MCP server")]
    public void GivenARunningMcpServer()
    {
        _fixture = new McpServerFixture();
    }

    [Then(@"the result contains ""(.+)""")]
    public void ThenTheResultContains(string value)
    {
        Assert.NotNull(_toolResult);
        Assert.Contains(value, _toolResult, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _response?.Dispose();
        _fixture?.Dispose();
        GC.SuppressFinalize(this);
    }
}
