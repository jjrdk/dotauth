namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
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

    // In order to secure access to a resource
    // As a resource owner
    // I want to log in using resource owner flow

    // In order to secure access to a resource
    // As a resource owner
    // I want to log in using resource owner flow
    public class ResourceOwnerLoginFlowFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Successful authorization")]
        public void SuccessfulResourceOwnerAuthentication()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly configured token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("client", "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "openid" }))
                        .ConfigureAwait(false);
                    result = response.Content;

                    Assert.NotNull(result);
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken)token).Claims);
                });

            "and has valid id token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = TestKeys.SecretKey.CreateJwk(
                            JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify),
                        ValidAudience = "client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.IdToken, validationParameters, out _);
                });
        }

        [Scenario(DisplayName = "Successful token refresh")]
        public void SuccessfulResourceOwnerRefresh()
        {
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("client", "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "openid" }, "pwd"))
                        .ConfigureAwait(false);
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
            TokenClient client = null;
            GrantedTokenResponse result = null;

            "and a properly token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("client", "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "openid" }, "pwd"))
                        .ConfigureAwait(false);
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
            TokenClient client = null;
            BaseSidContentResult<GrantedTokenResponse> result = null;

            "and a token client with invalid client credentials".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("xxx", "xxx"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    result = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "openid" }, "pwd"))
                        .ConfigureAwait(false);
                });

            "then does not have token".x(() => { Assert.True(result.ContainsError); });
        }

        [Scenario(DisplayName = "Invalid user credentials")]
        public void InvalidUserCredentials()
        {
            TokenClient client = null;
            BaseSidContentResult<GrantedTokenResponse> result = null;

            "and a token client with invalid client credentials".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromBasicAuthentication("client", "client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting auth token".x(
                async () =>
                {
                    result = await client.GetToken(TokenRequest.FromPassword("someone", "xxx", new[] { "openid" }, "pwd"))
                        .ConfigureAwait(false);
                });

            "then does not have token".x(() => { Assert.True(result.ContainsError); });
        }
    }
}
