namespace DotAuth.AcceptanceTests.Features;

using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class ResourceOwnerCreationFeature : AuthorizedManagementFeatureBase
{
    /// <inheritdoc />
    public ResourceOwnerCreationFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Manager created user")]
    public void ManagerCreatedUser()
    {
        GrantedTokenResponse userToken = null!;

        "When manager creates user".x(
            async () =>
            {
                var created = await _managerClient.AddResourceOwner(
                        new AddResourceOwnerRequest {Subject = "tester", Password = "tester"},
                        _administratorToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;

                Assert.Equal("tester", created.Item.Subject);
            });

        "and user logs in".x(
            async () =>
            {
                var response = await _tokenClient
                    .GetToken(TokenRequest.FromPassword("tester", "tester", new[] {"openid"}))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                userToken = response.Item;
            });

        "then user has custom user claim".x(
            () =>
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(
                    userToken.AccessToken,
                    new NoOpTokenValidationParameters(SharedContext.Instance),
                    out var token);

                Assert.Contains((token as JwtSecurityToken).Claims, c => c.Type == "acceptance_test");
            });
    }
}