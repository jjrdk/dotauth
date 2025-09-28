namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.AcceptanceTests.Support;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private Client[] _clients = null!;
    private Option<Client>? _addClientResponse;

    [Given(@"a manager client")]
    public async Task GivenAManagerClient()
    {
        _managerClient = await ManagementClient.Create(_fixture!.Client, new Uri(WellKnownUmaConfiguration));
    }

    [Given(@"a manager token")]
    public async Task GivenAManagerToken()
    {
        var response = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", ["manager", "offline"]))
            ;

        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(response);

        _token = result.Item;
    }

    [When(@"getting all clients")]
    public async Task WhenGettingAllClients()
    {
        var option = await _managerClient.GetAllClients(_token.AccessToken);

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
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" }],
            AllowedScopes = ["test"],
            RedirectionUrls = [new Uri("http://localhost/callback")],
            ApplicationType = ApplicationTypes.Native,
            GrantTypes = [GrantTypes.ClientCredentials],
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet()
        };
        _addClientResponse = await _managerClient.AddClient(client, _token.AccessToken)
            ;
    }

    [Then(@"operation succeeds")]
    public void ThenOperationSucceeds()
    {
        Assert.IsType<Option<Client>.Result>(_addClientResponse);
    }
}
