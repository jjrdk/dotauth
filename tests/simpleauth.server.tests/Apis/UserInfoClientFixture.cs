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
        private readonly UserInfoClient _userInfoClient;

        public UserInfoClientFixture()
        {
            _server = new TestOauthServerFixture();
            _userInfoClient = new UserInfoClient(_server.Client);
        }

        [Fact]
        public async Task When_Pass_Invalid_Token_To_UserInfo_Then_Error_Is_Returned()
        {
            var getUserInfoResult = await _userInfoClient
                .Resolve(BaseUrl + WellKnownOpenidConfiguration, "invalid_access_token")
                .ConfigureAwait(false);

            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("the token is not valid", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Client_Access_Token_To_UserInfo_Then_Error_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(TokenRequest.FromScopes("openid")).ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient
                .Resolve(BaseUrl + WellKnownOpenidConfiguration, result.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("Not a valid resource owner token", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Json_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient.GetToken(
                    TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient
                .Resolve(BaseUrl + WellKnownOpenidConfiguration, result.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jws_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client_userinfo_sig_rs256", "client_userinfo_sig_rs256"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient
                .Resolve(BaseUrl + WellKnownOpenidConfiguration, result.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult.Content);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jwe_Is_Returned()
        {
            var tokenClient = await TokenClient.Create(
                    TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
                    _server.Client,
                    new Uri(BaseUrl + WellKnownOpenidConfiguration))
                .ConfigureAwait(false);
            var result = await tokenClient
                .GetToken(TokenRequest.FromPassword("administrator", "password", new[] { "scim" }))
                .ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient
                .Resolve(BaseUrl + WellKnownOpenidConfiguration, result.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.NotNull(getUserInfoResult.Content);
        }
    }
}
