namespace DotAuth.AcceptanceTests.StepDefinitions;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    [When(@"getting token")]
    public async Task WhenGettingToken()
    {
        var scopesToTry = new[] {
            new[] { "openid" },
            new[] { "api" },
            new[] { "api1" },
        };

        Option<GrantedTokenResponse>? resultOption = null;
        foreach (var scopes in scopesToTry)
        {
            var option = await _tokenClient.GetToken(TokenRequest.FromPassword("administrator", "password", scopes)).ConfigureAwait(false);
            if (option is Option<GrantedTokenResponse>.Result)
            {
                resultOption = option;
                break;
            }
        }

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(resultOption);
        _token = response.Item;

        Assert.NotNull(_token);
    }

    [Then(@"token has single audience")]
    public void ThenTokenHasSingleAudience()
    {
        Assert.NotNull(_token);
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
        Assert.NotNull(_token);
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(_token.IdToken, validationParameters, out _);
    }
}
