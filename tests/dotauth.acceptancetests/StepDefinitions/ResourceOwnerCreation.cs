namespace DotAuth.AcceptanceTests.StepDefinitions;

using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.AcceptanceTests.Support;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [When(@"manager creates user")]
    public async Task WhenManagerCreatesUser()
    {
        var option = await _managerClient.AddResourceOwner(
                new AddResourceOwnerRequest {Subject = "tester", Password = "tester"},
                _token.AccessToken)
            ;
        var created = Assert.IsType<Option<AddResourceOwnerResponse>.Result>(option);

        Assert.Equal("tester", created.Item.Subject);
    }

    [When(@"user logs in")]
    public async Task WhenUserLogsIn()
    {
        var option = await _tokenClient
            .GetToken(TokenRequest.FromPassword("tester", "tester", new[] {"openid"}))
            ;
        var token = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _token = token.Item;
    }

    [Then(@"user has custom user claim")]
    public void ThenUserHasCustomUserClaim()
    {
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(
            _token.AccessToken,
            new NoOpTokenValidationParameters(SharedContext.Instance),
            out var token);

        Assert.Contains((token as JwtSecurityToken)!.Claims, c => c.Type == "acceptance_test");
    }

    [Given(@"an manager token")]
    public async Task GivenAnManagerToken()
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", new[] {"manager", "offline"}))
            ;
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        Assert.NotNull(result.Item);

        _token = result.Item;
    }
}
