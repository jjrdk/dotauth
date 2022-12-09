namespace DotAuth.AcceptanceTests.Features;

using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public abstract class AuthFlowFeature
{
    protected readonly ITestOutputHelper _outputHelper;
    protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    protected const string BaseUrl = "http://localhost:5000";
    protected TestServerFixture Fixture = null!;
    protected JsonWebKeySet ServerKeyset = null!;

    public AuthFlowFeature(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    [Background]
    public void Background()
    {
        "Given a running auth server".x(() => Fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => Fixture?.Dispose());

        "And the server signing keys".x(
            async () =>
            {
                var keysJson = await Fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(keysJson);

                ServerKeyset = keys!;
                Assert.NotEmpty(ServerKeyset?.Keys);
            });
    }
}