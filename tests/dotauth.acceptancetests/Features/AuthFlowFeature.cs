namespace DotAuth.AcceptanceTests.Features;

using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

[Binding]
public partial class AuthFlowFeature
{
    private readonly ITestOutputHelper _outputHelper;
    public const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    public const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";
    protected const string BaseUrl = "http://localhost:5000";
    private TestServerFixture _fixture = null!;
    private JsonWebKeySet _serverKeyset = null!;

    public AuthFlowFeature(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    [Given(@"a running auth server")]
    public void GivenARunningAuthServer()
    {
        _fixture = new TestServerFixture(_outputHelper, BaseUrl);
    }

    [Given(@"the server's signing key")]
    public async Task GivenTheServersSigningKey()
    {
        var json = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
        _serverKeyset = new JsonWebKeySet(json);

        Assert.NotEmpty(_serverKeyset.Keys);
    }

    [Background]
    public void Background()
    {
        "Given a running auth server".x(() => _fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => _fixture?.Dispose());

        "And the server signing keys".x(
            async () =>
            {
                var keysJson = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(keysJson);

                _serverKeyset = keys!;
                Assert.NotEmpty(_serverKeyset?.Keys);
            });
    }
}