namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class ClientCredentialsLoginFlowFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public ClientCredentialsLoginFlowFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Successful authorization")]
    public void SuccessfulClientCredentialsAuthentication()
    {
        TokenClient client = null!;
        GrantedTokenResponse result = null!;

        "and a properly configured token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting token".x(
            async () =>
            {
                var response =
                    await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                        Option<GrantedTokenResponse>.Result;

                Assert.NotNull(response);

                result = response.Item;
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

        "and can get user info".x(
            async () =>
            {
                var userinfo = await client.GetUserInfo(result.AccessToken).ConfigureAwait(false) as Option<JwtPayload>.Result;

                Assert.NotNull(userinfo);
                Assert.NotNull(userinfo.Item);
            });
    }

    [Scenario(DisplayName = "Successful token refresh")]
    public void SuccessfulResourceOwnerRefresh()
    {
        TokenClient client = null!;
        GrantedTokenResponse result = null!;

        "and a properly token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromBasicAuthentication("clientCredentials", "clientCredentials"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting auth token".x(
            async () =>
            {
                var response = await client.GetToken(TokenRequest.FromScopes("api1", "offline"))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(response);

                result = response.Item;
            });

        "then can get new token from refresh token".x(
            async () =>
            {
                var response = await client.GetToken(TokenRequest.FromRefreshToken(result.RefreshToken))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                Assert.NotNull(response);
            });
    }

    [Scenario(DisplayName = "Successful token revocation")]
    public void SuccessfulResourceOwnerRevocation()
    {
        TokenClient client = null!;
        GrantedTokenResponse result = null!;

        "and a properly token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting auth token".x(
            async () =>
            {
                var response =
                    await client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                        Option<GrantedTokenResponse>.Result;

                Assert.NotNull(response);

                result = response.Item;
            });

        "then can revoke token".x(
            async () =>
            {
                var response = await client.RevokeToken(RevokeTokenRequest.Create(result)).ConfigureAwait(false);
                Assert.IsType<Option.Success>(response);
            });
    }

    [Scenario(DisplayName = "Invalid client")]
    public void InvalidClientCredentials()
    {
        TokenClient client = null!;
        Option<GrantedTokenResponse> result = null!;

        "and a token client with invalid client credentials".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials("xxx", "xxx"),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting auth token".x(
            async () => { result = await client.GetToken(TokenRequest.FromScopes("pwd")).ConfigureAwait(false); });

        "then does not have token".x(() => { Assert.IsType<Option<GrantedTokenResponse>.Error>(result); });
    }
}