namespace DotAuth.Stores.Redis.AcceptanceTests.Features;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    [When(@"getting token")]
    public async Task WhenGettingToken()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "api" }))
            .ConfigureAwait(false);
        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);
        _token = response.Item;

        Assert.NotNull(_token);
    }

    [Then(@"token has single audience")]
    public void ThenTokenHasSingleAudience()
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(_token.IdToken);
        Assert.Equal("no_key", string.Join('$', jwt.Audiences));
    }

    [Then(@"token is signed with server key")]
    public void ThenTokenIsSignedWithServerKey()
    {
        var key = _serverKeySet.GetSigningKeys().First();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateAudience = false,
            ValidateActor = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateTokenReplay = false
        };
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(_token.IdToken, validationParameters, out _);
    }
}
