namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
public partial class FeatureTest : IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    public const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    public const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";
    protected const string BaseUrl = "http://localhost:5000";
    private TestServerFixture _fixture = null!;
    private JsonWebKeySet _serverKeyset = null!;
    protected ManagementClient _managerClient = null!;
    protected GrantedTokenResponse _administratorToken = null!;

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
        var json = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
        _serverKeyset = new JsonWebKeySet(json);

        Assert.NotEmpty(_serverKeyset.Keys);
    }

    [Given(@"a client credentials token client with (.+), (.+)")]
    public void GivenAClientCredentialsTokenClientWith(string id, string secret)
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(id, secret),
            _fixture.Client,
            new Uri(FeatureTest.WellKnownOpenidConfiguration));
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