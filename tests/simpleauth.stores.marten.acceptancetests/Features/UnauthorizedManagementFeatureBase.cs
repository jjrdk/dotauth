namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;
    using System;
    using Microsoft.Extensions.Configuration;
    using SimpleAuth.Shared;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class UnauthorizedManagementFeatureBase
    {
        private readonly ITestOutputHelper _output;
        private const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new(BaseUrl + "/.well-known/openid-configuration");
        protected TestServerFixture Fixture = null!;
        protected ManagementClient ManagerClient = null!;
        protected TokenClient TokenClient = null!;
        protected GrantedTokenResponse GrantedToken = null!;
        protected string ConnectionString = null!;

        public UnauthorizedManagementFeatureBase(ITestOutputHelper output)
        {
            _output = output;
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

                    Assert.NotNull(ConnectionString);
                });

            "Given a configured database".x(
                    async () =>
                    {
                        ConnectionString = await DbInitializer.Init(
                                _output,
                                ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(SharedContext.Instance),
                                DefaultStores.Scopes())
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => { await DbInitializer.Drop(ConnectionString, _output).ConfigureAwait(false); });

            "and a running auth server".x(() => Fixture = new TestServerFixture(_output, ConnectionString, BaseUrl))
                .Teardown(() => Fixture.Dispose());

            "and a manager client".x(
                async () =>
                {
                    ManagerClient = await ManagementClient.Create(Fixture.Client, WellKnownUmaConfiguration)
                        .ConfigureAwait(false);
                });

            "and a token client".x(
                () =>
                {
                    TokenClient = new TokenClient(
                        TokenCredentials.FromClientCredentials("admin_client", "admin_client"),
                        Fixture.Client,
                        WellKnownUmaConfiguration);
                });

            "and an admin token".x(
                async () =>
                {
                    var result =
                        await TokenClient.GetToken(TokenRequest.FromScopes("admin")).ConfigureAwait(false) as
                            Option<GrantedTokenResponse>.Result;

                    Assert.NotNull(result.Item);

                    GrantedToken = result.Item;
                });
        }
    }
}
