namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Client.Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using Xbehave;
    using Xunit;

    public class ClientCredentialsLoginFlowFeature
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "https://localhost/.well-known/openid-configuration";

        public ClientCredentialsLoginFlowFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Scenario(DisplayName = "Successful authorization")]
        public void SuccessfulClientCredentialsAuthentication()
        {
            TestServerFixture fixture = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a properly configured token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = TestKeys.SecretKey.CreateJwk(
                            JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify),
                        ValidAudience = "clientCredentials",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);
                });
        }

        [Scenario(DisplayName = "Successful token refresh")]
        public void SuccessfulResourceOwnerRefresh()
        {
            TestServerFixture fixture = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a properly token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("clientCredentials", "clientCredentials"),
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    result = response.Content;
                });

            "then can get new token from refresh token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromRefreshToken(result.RefreshToken))
                        .ConfigureAwait(false);
                    Assert.False(response.ContainsError);
                });
        }

        [Scenario(DisplayName = "Successful token revocation")]
        public void SuccessfulResourceOwnerRevocation()
        {
            TestServerFixture fixture = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a properly token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    result = response.Content;
                });

            "then can revoke token".x(
                async () =>
                {
                    var response = await client.RevokeToken(RevokeTokenRequest.Create(result))
                        .ConfigureAwait(false);
                    Assert.Equal(HttpStatusCode.OK, response.Status);
                });
        }

        [Scenario(DisplayName = "Invalid client")]
        public void InvalidClientCredentials()
        {
            TestServerFixture fixture = null;
            TokenClient client = null;
            BaseSidContentResult<GrantedTokenResponse> result = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a token client with invalid client credentials".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromClientCredentials("xxx", "xxx"),
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () => { result = await client.GetToken(TokenRequest.FromScopes("pwd")).ConfigureAwait(false); });

            "then does not have token".x(() => { Assert.Null(result.Content); });
        }
    }
}
