namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private GrantedTokenResponse _token = null!;
    private Option<GrantedTokenResponse> _tokenOption = null!;

    [When(@"requesting token")]
    public async Task WhenRequestingToken()
    {
        var option =
            await _tokenClient.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _token = response.Item;
    }

    [Then(@"has valid access token")]
    public void ThenHasValidAccessToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = _serverKeySet!.GetSigningKeys(),
            ValidAudience = "clientCredentials",
            ValidIssuer = "https://localhost"
        };
        var principal = tokenHandler.ValidateToken(_token.AccessToken, validationParameters, out _);

        Assert.NotNull(principal);
    }

    [Then(@"can get user info")]
    public async Task ThenCanGetUserInfo()
    {
        var option = await _tokenClient.GetUserInfo(_token.AccessToken).ConfigureAwait(false);
        var userinfo = Assert.IsType<Option<JwtPayload>.Result>(option);
        Assert.NotNull(userinfo);
        Assert.NotNull(userinfo.Item);
    }

    [When(@"requesting auth token")]
    public async Task WhenRequestingAuthToken()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromScopes("api1", "offline")).ConfigureAwait(false);

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _token = response.Item;
    }

    [When(@"attempting to request token")]
    public async Task WhenAttemptingToRequestToken()
    {
        _tokenOption = await _tokenClient.GetToken(TokenRequest.FromScopes("pwd")).ConfigureAwait(false);
    }

    [Then(@"can get new token from refresh token")]
    public async Task ThenCanGetNewTokenFromRefreshToken()
    {
        var response = await _tokenClient.GetToken(TokenRequest.FromRefreshToken(_token.RefreshToken!))
            .ConfigureAwait(false);
        Assert.IsType<Option<GrantedTokenResponse>.Result>(response);
    }

    [Then(@"can revoke token")]
    public async Task ThenCanRevokeToken()
    {
        var response = await _tokenClient.RevokeToken(RevokeTokenRequest.Create(_token)).ConfigureAwait(false);
        Assert.IsType<Option.Success>(response);
    }

    [Given(@"a token client with invalid client credentials")]
    public void GivenATokenClientWithInvalidClientCredentials()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("xxx", "xxx"),
            _fixture!.Client,
            new Uri(FeatureTest.WellKnownOpenidConfiguration));
    }

    [Then(@"does not have token")]
    public void ThenDoesNotHaveToken()
    {
        Assert.IsType<Option<GrantedTokenResponse>.Error>(_tokenOption);
    }
}
