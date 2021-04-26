namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Npgsql;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class AuthFlowFeature
    {
        protected readonly ITestOutputHelper _outputHelper;
        protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
        protected const string BaseUrl = "http://localhost:5000";
        protected TestServerFixture _fixture = null;
        protected JsonWebKeySet _jwks = null;
        protected string _connectionString = null;

        public AuthFlowFeature(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
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
                    _connectionString = configuration["Db:ConnectionString"];
                    _outputHelper.WriteLine(_connectionString);
                    Assert.NotNull(_connectionString);
                });

            "Given a configured database".x(
                    async () =>
                    {
                        _connectionString = await DbInitializer.Init(
                                _outputHelper,
                                _connectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(SharedContext.Instance),
                                DefaultStores.Scopes())
                            .ConfigureAwait(false);
                        var builder = new NpgsqlConnectionStringBuilder(_connectionString);

                        Assert.False(string.IsNullOrWhiteSpace(builder.SearchPath));
                        _outputHelper.WriteLine(_connectionString);
                    })
                .Teardown(async () => { await DbInitializer.Drop(_connectionString).ConfigureAwait(false); });

            "and a running auth server"
                .x(() => _fixture = new TestServerFixture(_outputHelper, _connectionString, BaseUrl))
                .Teardown(() => _fixture.Dispose());

            "And the server signing keys".x(
                async () =>
                {
                    var keysJson = await _fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                    _jwks = new JsonWebKeySet(keysJson);

                    Assert.NotEmpty(_jwks.Keys);
                });
        }
    }
}
