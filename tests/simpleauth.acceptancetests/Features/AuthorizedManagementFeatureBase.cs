namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class AuthorizedManagementFeatureBase
    {
        private readonly ITestOutputHelper _outputHelper;
        private const string BaseUrl = "http://localhost";
        private static readonly Uri WellKnownUmaConfiguration = new(BaseUrl + "/.well-known/openid-configuration");
        protected TestServerFixture _fixture = null!;
        protected ManagementClient _managerClient = null!;
        protected TokenClient _tokenClient = null!;
        protected GrantedTokenResponse _administratorToken = null!;

        public AuthorizedManagementFeatureBase(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Background]
        public void Background()
        {
            "Given a running auth server".x(() => _fixture = new TestServerFixture(_outputHelper, BaseUrl))
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
                    var result = await _tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "manager" })).ConfigureAwait(false);

                    Assert.NotNull(result.Content);

                    _administratorToken = result.Content;
                });
        }
    }
}