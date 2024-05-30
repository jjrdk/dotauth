namespace DotAuth.Server.Tests.Apis;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Xunit;
using Xunit.Abstractions;

public sealed class UserInfoClientFixture
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
    private readonly TestOauthServerFixture _server;
    private readonly TokenClient _userInfoClient;

    public UserInfoClientFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestOauthServerFixture(outputHelper);
        _userInfoClient = new TokenClient(
            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
            _server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
    }

    [Fact]
    public async Task When_Pass_Invalid_Token_To_UserInfo_Then_Error_Is_Returned()
    {
        var getUserInfoResult = Assert.IsType<Option<JwtPayload>.Error>(
            await _userInfoClient.GetUserInfo("invalid_access_token"));

        Assert.Equal("invalid_token", getUserInfoResult.Details.Title);
        Assert.Equal(Strings.TheTokenIsNotValid, getUserInfoResult.Details.Detail);
    }

    [Fact]
    public async Task WhenPassingClientAccessTokenToUserInfoThenClientClaimsAreReturned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result =
            Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
                .GetToken(TokenRequest.FromScopes("openid")));
        var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Item.AccessToken);

        Assert.IsType<Option<JwtPayload>.Result>(getUserInfoResult);
    }

    [Fact]
    public async Task When_Pass_Access_Token_Then_Json_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient.GetToken(
                TokenRequest.FromPassword("administrator", "password", ["scim"]))
            );

        var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Item.AccessToken);

        Assert.IsType<Option<JwtPayload>.Result>(getUserInfoResult);
    }

    [Fact]
    public async Task When_Pass_Access_Token_Then_Jws_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client_userinfo_sig_rs256", "client_userinfo_sig_rs256"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", ["scim"]))
            );

        var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Item.AccessToken);

        Assert.IsType<Option<JwtPayload>.Result>(getUserInfoResult);
    }

    [Fact]
    public async Task When_Pass_Access_Token_Then_Jwe_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
            _server.Client,
            new Uri(BaseUrl + WellKnownOpenidConfiguration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromPassword("administrator", "password", ["scim"]))
            );

        var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Item.AccessToken);

        Assert.IsType<Option<JwtPayload>.Result>(getUserInfoResult);
    }
}
