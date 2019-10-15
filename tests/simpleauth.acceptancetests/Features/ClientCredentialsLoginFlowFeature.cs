namespace SimpleAuth.AcceptanceTests.Features
{
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Client.Results;
    using SimpleAuth.Shared.Responses;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using Xbehave;
    using Xunit;

    public class ClientCredentialsLoginFlowFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Successful authorization")]
        public void SuccessfulClientCredentialsAuthentication()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly configured token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.HasError);

                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "clientCredentials",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);
                });
        }

        [Scenario(DisplayName = "Successful token refresh")]
        public void SuccessfulResourceOwnerRefresh()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromBasicAuthentication("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.HasError);

                    result = response.Content;
                });

            "then can get new token from refresh token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromRefreshToken(result.RefreshToken))
                        .ConfigureAwait(false);
                    Assert.False(response.HasError);
                });
        }

        [Scenario(DisplayName = "Successful token revocation")]
        public void SuccessfulResourceOwnerRevocation()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly token client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false);

                    Assert.False(response.HasError);

                    result = response.Content;
                });

            "then can revoke token".x(
                async () =>
                {
                    var response = await client.RevokeToken(RevokeTokenRequest.Create(result)).ConfigureAwait(false);
                    Assert.Equal(HttpStatusCode.OK, response.Status);
                });
        }

        [Scenario(DisplayName = "Invalid client")]
        public void InvalidClientCredentials()
        {
            TokenClient client = null;
            BaseSidContentResult<GrantedTokenResponse> result = null;

            "and a token client with invalid client credentials".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials("xxx", "xxx"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting auth token".x(
                async () => { result = await client.GetToken(TokenRequest.FromScopes("pwd")).ConfigureAwait(false); });

            "then does not have token".x(() => { Assert.Null(result.Content); });
        }
    }
}
