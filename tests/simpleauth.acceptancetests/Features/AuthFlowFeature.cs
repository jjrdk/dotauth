namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Logging;
    using Xbehave;

    public abstract class AuthFlowFeature
    {
        protected const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";
        protected const string BaseUrl = "http://localhost:5000";
        protected TestServerFixture fixture = null;

        public AuthFlowFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Background]
        public void Background()
        {

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

        }
    }
}