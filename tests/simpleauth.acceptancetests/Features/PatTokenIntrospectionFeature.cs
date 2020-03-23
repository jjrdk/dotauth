namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class PatTokenIntrospectionFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Can register a resource for a user and manage policies")]
        public void CanGetIntrospectionInfoFromPatToken()
        {
            TokenClient client = null;
            UmaClient umaClient = null;
            string token = null;

            "Given a token client".x(
                () =>
                {
                    client = new TokenClient(
                        TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration));
                });

            "And a UMA client".x(() => { umaClient = new UmaClient(_fixture.Client, new Uri(BaseUrl)); });

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

                    Assert.False(userInfo.ContainsError);
                    Assert.NotNull(userInfo.Content.Sub);
                });
        }
    }
}