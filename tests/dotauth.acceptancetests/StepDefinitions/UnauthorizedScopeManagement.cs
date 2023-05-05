namespace DotAuth.AcceptanceTests.StepDefinitions;

using System.Net;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [Then(@"error is returned")]
    public void ThenErrorIsReturned()
    {
        var scope = Assert.IsType<Option<Scope>.Error>(_scope);

        Assert.Equal(HttpStatusCode.Forbidden, scope.Details.Status);
    }

    [When(@"adding new scope")]
    public async Task WhenAddingNewScope()
    {
        _scope = await _managerClient.AddScope(
                new Scope { Name = "test", Claims = new[] { "openid" } },
                _token.AccessToken)
            .ConfigureAwait(false);
    }
}