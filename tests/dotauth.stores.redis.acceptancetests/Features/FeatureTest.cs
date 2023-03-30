namespace DotAuth.Stores.Redis.AcceptanceTests.Features;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
public partial class FeatureTest : IAsyncDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";
    private const string BaseUrl = "http://localhost:5000";
    private TestServerFixture _fixture = null!;
    private JsonWebKeySet _serverKeySet = null!;
    private ManagementClient _managerClient = null!;
    private string _connectionString = null!;

    public FeatureTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        #if DEBUG
        IdentityModelEventSource.ShowPII = true;
        #endif
    }

    [BeforeScenario(Order = 1)]
    public async Task SetupConnectionString()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
        _connectionString = configuration["Db:ConnectionString"]!;
        Assert.False(string.IsNullOrWhiteSpace(_connectionString));
    }

    [BeforeScenario(Order = 10)]
    public async Task SetupDatabase()
    {
        _connectionString = await DbInitializer.Init(
                _outputHelper,
                _connectionString!,
                DefaultStores.Consents(),
                DefaultStores.Users(),
                DefaultStores.Clients(SharedContext.Instance),
                DefaultStores.Scopes())
            .ConfigureAwait(false);
        // var builder = new NpgsqlConnectionStringBuilder(_connectionString);
    }

    [AfterScenario]
    public async Task Teardown()
    {
        await DbInitializer.Drop(_connectionString).ConfigureAwait(false);
    }

    [Given(@"a running auth server")]
    public async Task GivenARunningAuthServer()
    {
        _fixture = new TestServerFixture(_outputHelper, _connectionString, BaseUrl);
    }

    [Given(@"the server's signing key")]
    public async Task GivenTheServersSigningKey()
    {
        var json = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
        _serverKeySet = new JsonWebKeySet(json);

        Assert.NotEmpty(_serverKeySet.Keys);
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
    public async ValueTask DisposeAsync()
    {
        _fixture?.Dispose();
        _responseMessage?.Dispose();
        _pollingTask?.Dispose();
        await DbInitializer.Drop(_connectionString).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
