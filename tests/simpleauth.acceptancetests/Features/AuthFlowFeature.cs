namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Xbehave;
    using Xunit;

    public abstract class AuthFlowFeature
    {
        protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
        protected const string BaseUrl = "http://localhost:5000";
        protected TestServerFixture fixture = null;
        protected JsonWebKeySet jwks = null;

        public AuthFlowFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Background]
        public void Background()
        {
            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "And the server signing keys".x(
                async () =>
                {
                    var keysJson = await fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                    jwks = new JsonWebKeySet(keysJson);

                    Assert.NotEmpty(jwks.Keys);
                });
        }
    }
}