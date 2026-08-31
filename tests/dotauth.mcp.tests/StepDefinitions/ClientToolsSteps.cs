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
    private ClientTools _clientTools = null!;

    [Given(@"clients ""(.+)"" and ""(.+)"" are registered")]
    public void GivenClientsAreRegistered(string id1, string id2)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ClientStore.GetAll(Arg.Any<CancellationToken>())
            .Returns(
            [
                new Client { ClientId = id1, ClientName = $"Client {id1}" },
                new Client { ClientId = id2, ClientName = $"Client {id2}" }
            ]);
        _clientTools = new ClientTools(_fixture.ClientStore);
    }

    [Given(@"no client with id ""(.+)"" is registered")]
    public void GivenNoClientIsRegistered(string clientId)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ClientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);
        _clientTools = new ClientTools(_fixture.ClientStore);
    }

    [Given(@"a client with id ""(.+)"" is registered")]
    public void GivenAClientIsRegistered(string clientId)
    {
        _fixture ??= new McpServerFixture();
        _fixture.ClientStore.GetById(clientId, Arg.Any<CancellationToken>())
            .Returns(new Client { ClientId = clientId, ClientName = $"Client {clientId}" });
        _clientTools = new ClientTools(_fixture.ClientStore);
    }

    [When(@"list_clients is invoked")]
    public async Task WhenListClientsIsInvoked()
    {
        _toolResult = await _clientTools.ListClients(CancellationToken.None);
    }

    [When(@"get_client is invoked with id ""(.+)""")]
    public async Task WhenGetClientIsInvokedWithId(string clientId)
    {
        _toolResult = await _clientTools.GetClient(clientId, CancellationToken.None);
    }

    [Then(@"the result does not contain the secrets field")]
    public void ThenTheResultDoesNotContainTheSecretsField()
    {
        Assert.NotNull(_toolResult);
        Assert.DoesNotContain("\"secrets\"", _toolResult, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"the result indicates the client was not found")]
    public void ThenTheResultIndicatesTheClientWasNotFound()
    {
        Assert.NotNull(_toolResult);
        Assert.Contains("not found", _toolResult, StringComparison.OrdinalIgnoreCase);
    }
}
