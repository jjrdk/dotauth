namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.AcceptanceTests.Support;
using DotAuth.Client;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
public partial class FeatureTest : IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";
    private const string BaseUrl = "http://localhost:5000";
    private TestServerFixture _fixture = null!;
    private JsonWebKeySet _serverKeySet = null!;
    private ManagementClient _managerClient = null!;

    public FeatureTest(ITestOutputHelper outputHelper)
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
        var json = await _fixture.Client().GetStringAsync($"{BaseUrl}/jwks");
        _serverKeySet = new JsonWebKeySet(json);

        Assert.NotEmpty(_serverKeySet.Keys);
    }

    [Given(@"a client credentials token client with (.+), (.+)")]
    public void GivenAClientCredentialsTokenClientWith(string id, string secret)
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(id, secret),
            _fixture.Client,
            new Uri(WellKnownOpenidConfiguration));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _fixture?.Dispose();
        _responseMessage?.Dispose();
        _pollingTask?.Dispose();
        GC.SuppressFinalize(this);
    }
}
