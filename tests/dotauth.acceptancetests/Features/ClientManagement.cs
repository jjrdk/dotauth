namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
[Scope(Feature = "Client Management")]
public class ClientManagement : AuthorizedManagementFeatureBase
{
    private Client[] _clients = null!;
    private Option<Client> _addClientResponse;

    /// <inheritdoc />
    public ClientManagement(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Given(@"a running auth server")]
    public void GivenARunningAuthServer()
    {
        Fixture = new TestServerFixture(OutputHelper, BaseUrl);
    }

    [Given(@"a manager client")]
    public async Task GivenAManagerClient()
    {
        _managerClient = await ManagementClient.Create(Fixture.Client, WellKnownUmaConfiguration).ConfigureAwait(false);
    }

    [Given(@"a token client")]
    public void GivenATokenClient()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("manager_client", "manager_client"),
            Fixture.Client,
            WellKnownUmaConfiguration);
    }

    [Given(@"a manager token")]
    public async Task GivenAManagerToken()
    {
        var response = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", new[] { "manager", "offline" }))
            .ConfigureAwait(false);

        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(response);

        _administratorToken = result.Item;
    }

    [When(@"getting all clients")]
    public async Task WhenGettingAllClients()
    {
        var option = await _managerClient.GetAllClients(_administratorToken.AccessToken).ConfigureAwait(false);

        var response = Assert.IsType<Option<Client[]>.Result>(option);

        _clients = response.Item;
    }

    [Then(@"contains list of clients")]
    public void ThenContainsListOfClients()
    {
        Assert.All(_clients, x => { Assert.NotNull(x.ClientId); });
    }

    [When(@"adding client")]
    public async Task WhenAddingClient()
    {
        var client = new Client
        {
            ClientId = "test_client",
            ClientName = "Test Client",
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
            AllowedScopes = new[] { "api" },
            RedirectionUrls = new[] { new Uri("http://localhost/callback"), },
            ApplicationType = ApplicationTypes.Native,
            GrantTypes = new[] { GrantTypes.ClientCredentials },
            JsonWebKeys = TestKeys.SuperSecretKey.CreateSignatureJwk().ToSet()
        };
        _addClientResponse = await _managerClient.AddClient(client, _administratorToken.AccessToken)
            .ConfigureAwait(false);
    }

    [Then(@"operation succeeds")]
    public void ThenOperationSucceeds()
    {
        Assert.IsType<Option<Client>.Result>(_addClientResponse);
    }
}
