namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private Option<Scope> _scope = null!;

    [When(@"requesting existing scope")]
    public async Task WhenRequestingExistingScope()
    {
        _scope = await _managerClient.GetScope("test", _token.AccessToken).ConfigureAwait(false);
    }

    [Then(@"scope information is returned")]
    public void ThenScopeInformationIsReturned()
    {
        var result = Assert.IsType<Option<Scope>.Result>(_scope);

        var scope = result.Item;
        Assert.Equal("test", scope.Name);
    }
}
