namespace DotAuth.Stores.Marten.AcceptanceTests.Features;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public abstract class AuthFlowFeature
{
    protected readonly ITestOutputHelper OutputHelper;
    protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
    protected const string BaseUrl = "http://localhost:5000";
    protected TestServerFixture Fixture = null!;
    protected JsonWebKeySet Jwks = null!;
    protected string ConnectionString = null!;

    public AuthFlowFeature(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    [Background]
    public void Background()
    {
        "Given loaded configuration values".x(
            () =>
            {
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false)
                    .Build();
                ConnectionString = configuration["Db:ConnectionString"];
                OutputHelper.WriteLine(ConnectionString);
                Assert.NotNull(ConnectionString);
            });

        "Given a configured database".x(
                async () =>
                {
                    ConnectionString = await DbInitializer.Init(
                            OutputHelper,
                            ConnectionString,
                            DefaultStores.Consents(),
                            DefaultStores.Users(),
                            DefaultStores.Clients(SharedContext.Instance),
                            DefaultStores.Scopes())
                        .ConfigureAwait(false);
                    var builder = new NpgsqlConnectionStringBuilder(ConnectionString);

                    Assert.False(string.IsNullOrWhiteSpace(builder.SearchPath));
                    OutputHelper.WriteLine(ConnectionString);
                })
            .Teardown(async () => { await DbInitializer.Drop(ConnectionString, OutputHelper).ConfigureAwait(false); });

        "and a running auth server"
            .x(() => Fixture = new TestServerFixture(OutputHelper, ConnectionString, BaseUrl))
            .Teardown(() => Fixture.Dispose());

        "And the server signing keys".x(
            async () =>
            {
                var keysJson = await Fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                Jwks = new JsonWebKeySet(keysJson);

                Assert.NotEmpty(Jwks.Keys);
            });
    }
}