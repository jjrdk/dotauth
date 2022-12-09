namespace DotAuth.AcceptanceTests.Features;

using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
[Scope(Feature = "Authorized Scope Management")]
public class AuthorizedScopeManagement : AuthorizedManagementFeatureBase
{
    private Scope scope = null!;

    /// <inheritdoc />
    public AuthorizedScopeManagement(ITestOutputHelper outputHelper)
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

    [When(@"requesting existing scope")]
    public async Task WhenRequestingExistingScope()
    {
        var response = await _managerClient.GetScope("test", _administratorToken.AccessToken).ConfigureAwait(false);

        var result = Assert.IsType<Option<Scope>.Result>(response);

        scope = result.Item;
    }

    [Then(@"scope information is returned")]
    public void ThenScopeInformationIsReturned()
    {
        Assert.Equal("test", scope.Name);
    }
}
