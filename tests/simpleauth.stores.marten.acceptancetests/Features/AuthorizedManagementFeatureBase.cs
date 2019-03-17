namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using System;
    using SimpleAuth.Client;
    using SimpleAuth.Manager.Client;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public abstract class AuthorizedManagementFeatureBase
    {
        private const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new Uri(BaseUrl + "/.well-known/openid-configuration");
        private string _connectionString = null;
        protected TestServerFixture _fixture = null;
        protected ManagementClient _managerClient = null;
        protected TokenClient _tokenClient = null;
        protected GrantedTokenResponse _grantedToken = null;

        [Background]
        public void Background()
        {
            "Given a configured database".x(
                    async () =>
                    {
                        _connectionString = await DbInitializer.Init(
                                TestData.ConnectionString,
                                DefaultStores.Consents(),
                                DefaultStores.Users(),
                                DefaultStores.Clients(SharedContext.Instance),
                                DefaultStores.Scopes())
                            .ConfigureAwait(false);
                    })
                .Teardown(async () => { await DbInitializer.Drop(_connectionString).ConfigureAwait(false); });

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
                            TokenCredentials.FromClientCredentials("manager_client", "manager_client"),
                            _fixture.Client,
                            WellKnownUmaConfiguration)
                        .ConfigureAwait(false);
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