namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;

public partial class AuthFlowFeature
{
    private GrantedTokenResponse _result = null!;
    private Option<GrantedTokenResponse> _tokenOption = null!;

    [Given(@"a properly configured token client")]
    public void GivenAProperlyConfiguredTokenClient()
    {
        _client = new TokenClient(
            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
            _fixture!.Client,
            new Uri(AuthFlowFeature.WellKnownUmaConfiguration));
    }

    [When(@"requesting token")]
    public async Task WhenRequestingToken()
    {
        var option =
            await _client.GetToken(TokenRequest.FromScopes("api1")).ConfigureAwait(false) as
                Option<GrantedTokenResponse>.Result;

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _result = response.Item;
    }

    [Then(@"has valid access token")]
    public void ThenHasValidAccessToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = _serverKeyset!.GetSigningKeys(),
            ValidAudience = "clientCredentials",
            ValidIssuer = "https://localhost"
        };
        var principal = tokenHandler.ValidateToken(_result.AccessToken, validationParameters, out _);

        Assert.NotNull(principal);
    }

    [Then(@"can get user info")]
    public async Task ThenCanGetUserInfo()
    {
        var option = await _client.GetUserInfo(_result.AccessToken).ConfigureAwait(false);
        var userinfo = Assert.IsType<Option<JwtPayload>.Result>(option);
        Assert.NotNull(userinfo);
        Assert.NotNull(userinfo.Item);
    }

    [When(@"requesting auth token")]
    public async Task WhenRequestingAuthToken()
    {
        var option = await _client.GetToken(TokenRequest.FromScopes("api1", "offline")).ConfigureAwait(false);

        var response = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        _result = response.Item;
    }

    [When(@"attempting to request token")]
    public async Task WhenAttemptingToRequestToken()
    {
        _tokenOption = await _client.GetToken(TokenRequest.FromScopes("pwd")).ConfigureAwait(false);
    }

    [Then(@"can get new token from refresh token")]
    public async Task ThenCanGetNewTokenFromRefreshToken()
    {
        var response = await _client.GetToken(TokenRequest.FromRefreshToken(_result.RefreshToken!))
            .ConfigureAwait(false);
        Assert.IsType<Option<GrantedTokenResponse>.Result>(response);
    }

    [Then(@"can revoke token")]
    public async Task ThenCanRevokeToken()
    {
        var response = await _client.RevokeToken(RevokeTokenRequest.Create(_result)).ConfigureAwait(false);
        Assert.IsType<Option.Success>(response);
    }

    [Given(@"a token client with invalid client credentials")]
    public void GivenATokenClientWithInvalidClientCredentials()
    {
        _client = new TokenClient(
            TokenCredentials.FromClientCredentials("xxx", "xxx"),
            _fixture!.Client,
            new Uri(AuthFlowFeature.WellKnownOpenidConfiguration));
    }

    [Then(@"does not have token")]
    public void ThenDoesNotHaveToken()
    {
        Assert.IsType<Option<GrantedTokenResponse>.Error>(_tokenOption);
    }
}
