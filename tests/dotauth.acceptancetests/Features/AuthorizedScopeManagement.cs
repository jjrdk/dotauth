namespace DotAuth.AcceptanceTests.Features;

using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

public partial class FeatureTest
{
    private Scope scope = null!;

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
