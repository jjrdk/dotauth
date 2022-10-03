namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class PatTokenUserInfoFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public PatTokenUserInfoFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Can get user info for PAT token")]
    public void CanGetUserInfoFromPatToken()
    {
        TokenClient client = null!;
        string token = null!;

        "Given a token client".x(
            () =>
            {
                client = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration));
            });

        "When getting a PAT token".x(
            async () =>
            {
                var response = await client.GetToken(
                        TokenRequest.FromPassword("administrator", "password", new[] { "uma_protection" }))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response.Item.AccessToken;

                Assert.NotNull(token);
            });

        "Then can get user information".x(
            async () =>
            {
                var userInfo = await client.GetUserInfo(token).ConfigureAwait(false)
                    as Option<JwtPayload>.Result;

                Assert.NotNull(userInfo);
                Assert.NotNull(userInfo.Item.Sub);
            });
    }
}