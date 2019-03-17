namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using SimpleAuth.Client;
    using SimpleAuth.Manager.Client;
    using SimpleAuth.Shared.Responses;
    using System;
    using Xbehave;
    using Xunit;

    public abstract class UnauthorizedManagementFeatureBase
    {
        private const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new Uri(BaseUrl + "/.well-known/openid-configuration");
        protected TestServerFixture _fixture = null;
        protected ManagementClient _managerClient = null;
        protected TokenClient _tokenClient = null;
        protected GrantedTokenResponse _grantedToken = null;
        protected string ConnectionString = null;

        [Background]
        public void Background()
        {
            "Given a configured database".x(
                    async () =>
                    {
                        ConnectionString = await DbInitializer.Init(
                                TestData.ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(SharedContext.Instance),
                                DefaultStores.Scopes())
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => { await DbInitializer.Drop(ConnectionString).ConfigureAwait(false); });

            "and a running auth server".x(() => _fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => _fixture.Dispose());

            "and a manager client".x(
                async () =>
                {
                    _managerClient = await ManagementClient.Create(_fixture.Client, WellKnownUmaConfiguration)
                        .ConfigureAwait(false);
                });

            "and a token client".x(
                async () =>
                {
                    _tokenClient = await TokenClient.Create(
                            TokenCredentials.FromClientCredentials("admin_client", "admin_client"),
                            _fixture.Client,
                            WellKnownUmaConfiguration)
                        .ConfigureAwait(false);
                });

            "and an admin token".x(
                async () =>
                {
                    var result = await _tokenClient.GetToken(TokenRequest.FromScopes("admin")).ConfigureAwait(false);

                    Assert.NotNull(result.Content);

                    _grantedToken = result.Content;
                });
        }
    }
}