namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using SimpleAuth.Client;
    using Xbehave;
    using Xunit;

    public class PatTokenUserInfoFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Can get user info for PAT token")]
        public void CanGetUserInfoFromPatToken()
        {
            TokenClient client = null;
            string token = null;

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
                        .ConfigureAwait(false);
                    token = response.Content.AccessToken;

                    Assert.NotNull(token);
                });

            "Then can get user information".x(
                async () =>
                {
                    var userInfo = await client.GetUserInfo(token).ConfigureAwait(false);

                    Assert.False(userInfo.HasError);
                    Assert.NotNull(userInfo.Content.Sub);
                });
        }
    }
}