namespace DotAuth.AcceptanceTests.Features;

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
                TokenRequest.FromPassword("administrator", "password", new[] { "uma_protection" }))
            .ConfigureAwait(false);
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        
        _token = response.Item;

        Assert.NotNull(_token);
    }

    [Then(@"can get user information")]
    public async Task ThenCanGetUserInformation()
    {
        var userInfo = await _tokenClient.GetUserInfo(_token.AccessToken).ConfigureAwait(false)
            as Option<JwtPayload>.Result;

        Assert.NotNull(userInfo);
        Assert.NotNull(userInfo.Item.Sub);
    }
}