namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Tokens;
    using Xbehave;
    using Xunit;

    public class JwksFeature
    {
        private const string BaseUrl = "http://localhost:5000";

        [Scenario]
        public void SuccessfulPermissionCreation()
        {
            TestServerFixture fixture = null;
            string jwksJson = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "then can download json web key set".x(
                async () =>
                {
                    jwksJson = await fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);

                    Assert.NotNull(jwksJson);
                });

            "and can create JWKS from json".x(
                () =>
                {
                    var jwks = new JsonWebKeySet(jwksJson);

                    Assert.NotEmpty(jwks.Keys);
                });
        }
    }
}
