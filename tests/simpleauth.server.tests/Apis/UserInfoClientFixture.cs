namespace SimpleAuth.Server.Tests.Apis
{
    using Client;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class UserInfoClientFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        private readonly TestOauthServerFixture _server;
        private readonly TokenClient _userInfoClient;

        public UserInfoClientFixture()
        {
            _server = new TestOauthServerFixture();
            _userInfoClient = new TokenClient(
                TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                _server.Client, new Uri(BaseUrl + WellKnownOpenidConfiguration));
        }

        [Fact]
        public async Task When_Pass_Invalid_Token_To_UserInfo_Then_Error_Is_Returned()
        {
            var getUserInfoResult = await _userInfoClient.GetUserInfo("invalid_access_token").ConfigureAwait(false);

            Assert.True(getUserInfoResult.HasError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Title);
            Assert.Equal("the token is not valid", getUserInfoResult.Error.Detail);
        }

        [Fact]
        public async Task WhenPassingClientAccessTokenToUserInfoThenClientClaimsAreReturned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient.GetToken(TokenRequest.FromScopes("openid")).ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Content.AccessToken).ConfigureAwait(false);

            Assert.False(getUserInfoResult.HasError);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Json_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "client"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Content.AccessToken).ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jws_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client_userinfo_sig_rs256", "client_userinfo_sig_rs256"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Content.AccessToken).ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult.Content);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jwe_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
                _server.Client,
                new Uri(BaseUrl + WellKnownOpenidConfiguration));
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.GetUserInfo(result.Content.AccessToken).ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult.Content);
        }
    }
}
