namespace SimpleAuth.AcceptanceTests.Features
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class ResourceOwnerCreationFeature : AuthorizedManagementFeatureBase
    {
        [Scenario(DisplayName = "Manager created user")]
        public void ManagerCreatedUser()
        {
            GrantedTokenResponse userToken = null;

            "When manager creates user".x(async () =>
            {
                var created = await _managerClient.AddResourceOwner(
                    new AddResourceOwnerRequest
                    {
                        Subject = "tester",
                        Password = "tester"
                    },
                    _grantedToken.AccessToken).ConfigureAwait(false);

                Assert.Equal("tester", created.Content);
            });

            "and user logs in".x(async () =>
            {
                var response = await _tokenClient
                    .GetToken(TokenRequest.FromPassword(
                        "tester",
                        "tester",
                        new[] { "openid" }))
                    .ConfigureAwait(false);

                userToken = response.Content;
            });

            "then user has custom user claim".x(() =>
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(
                    userToken.AccessToken,
                    new NoOpTokenValidationParameters(SharedContext.Instance),
                    out var token);

                Assert.True((token as JwtSecurityToken).Claims.Any(c => c.Type == "acceptance_test"));
            });
        }
    }
}