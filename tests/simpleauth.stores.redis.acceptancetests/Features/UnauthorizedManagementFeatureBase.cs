﻿namespace SimpleAuth.Stores.Redis.AcceptanceTests.Features
{
    using System;

    using Microsoft.Extensions.Configuration;

    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    using Xbehave;

    using Xunit;
    using Xunit.Abstractions;

    public abstract class UnauthorizedManagementFeatureBase
    {
        private readonly ITestOutputHelper _output;
        private const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new(BaseUrl + "/.well-known/openid-configuration");
        private TestServerFixture _fixture = null!;
        protected ManagementClient _managerClient = null!;
        private TokenClient _tokenClient = null!;
        protected GrantedTokenResponse _grantedToken = null!;
        private string _connectionString = null!;

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
                        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
                        _connectionString = configuration["Db:ConnectionString"];

                        Assert.NotNull(_connectionString);
                    });

            "Given a configured database".x(
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
                    })
                .Teardown(async () => { await DbInitializer.Drop(_connectionString).ConfigureAwait(false); });

            "and a running auth server".x(() => _fixture = new TestServerFixture(_output, _connectionString, BaseUrl))
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
                            TokenCredentials.FromClientCredentials("admin_client", "admin_client"),
                            _fixture.Client,
                            WellKnownUmaConfiguration);
                    });

            "and an admin token".x(
                async () =>
                    {
                        var result = await _tokenClient.GetToken(TokenRequest.FromScopes("admin"))
                                         .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                        Assert.NotNull(result.Item);

                        _grantedToken = result.Item;
                    });
        }
    }
}
