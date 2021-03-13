namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using Microsoft.Extensions.Configuration;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;
    using System;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class AuthorizedManagementFeatureBase
    {
        private readonly ITestOutputHelper _output;
        protected const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new Uri(BaseUrl + "/.well-known/openid-configuration");
        private string _connectionString = null;
        protected TestServerFixture _fixture = null;
        protected ManagementClient _managerClient = null;
        protected TokenClient _tokenClient = null;
        protected GrantedTokenResponse _grantedToken = null;

        public AuthorizedManagementFeatureBase(ITestOutputHelper output)
        {
            _output = output;
        }

        [Background]
        public void Background()
        {
            "Given loaded configuration values".x(
                () =>
                {
                    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
                    _connectionString = configuration["Db:ConnectionString"];

                    Assert.NotNull(_connectionString);
                });

            "and a configured database".x(
                    async () =>
                    {
                        _connectionString = await DbInitializer.Init(
                                _output,
                                _connectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(SharedContext.Instance),
                                DefaultStores.Scopes())
                            .ConfigureAwait(false);

                        Assert.NotNull(_connectionString);
                    })
                .Teardown(async () => { await DbInitializer.Drop(_connectionString).ConfigureAwait(false); });

            "and a running auth server".x(() => _fixture = new TestServerFixture(_connectionString, BaseUrl))
                .Teardown(() => _fixture.Dispose());

            "and a manager client".x(
                async () =>
                {
                    _managerClient = await ManagementClient.Create(_fixture.Client, WellKnownUmaConfiguration)
                        .ConfigureAwait(false);
                });

            "and a token client".x(
                () =>
                {
                    _tokenClient = new TokenClient(
                        TokenCredentials.FromClientCredentials("manager_client", "manager_client"),
                        _fixture.Client,
                        WellKnownUmaConfiguration);
                });

            "and a manager token".x(
                async () =>
                {
                    var result = await _tokenClient.GetToken(TokenRequest.FromScopes("manager")).ConfigureAwait(false);

                    Assert.NotNull(result.Content);

                    _grantedToken = result.Content;
                });
        }
    }
}