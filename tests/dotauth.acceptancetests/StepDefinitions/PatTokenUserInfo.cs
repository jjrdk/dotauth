namespace DotAuth.AcceptanceTests.StepDefinitions;

using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [When(@"getting a PAT token for (.+), (.+)")]
    public async Task WhenGettingAPatTokenFor(string user, string password)
    {
        var option = await _tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", ["uma_protection"]))
            ;
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        
        _token = response.Item;

        Assert.NotNull(_token);
    }

    [Then(@"can get user information")]
    public async Task ThenCanGetUserInformation()
    {
        var userInfo = Assert.IsType<Option<JwtPayload>.Result>(
            await _tokenClient.GetUserInfo(_token.AccessToken));

        Assert.NotNull(userInfo.Item.Sub);
    }
}
