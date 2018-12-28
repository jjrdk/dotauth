namespace SimpleAuth.Server.Tests.Apis
{
    using System.Threading.Tasks;
    using SimpleIdentityServer.Client;
    using SimpleIdentityServer.Client.Operations;
    using Xunit;

    public class UserInfoClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private IUserInfoClient _userInfoClient;

        public UserInfoClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Pass_Invalid_Token_To_UserInfo_Then_Error_Is_Returned()
        {            InitializeFakeObjects();

                        var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", "invalid_access_token").ConfigureAwait(false);

                        Assert.NotNull(getUserInfoResult);
            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("the token is not valid", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Client_Access_Token_To_UserInfo_Then_Error_Is_Returned()
        {            InitializeFakeObjects();

                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("openid"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken).ConfigureAwait(false);

                        Assert.NotNull(getUserInfoResult);
            Assert.True(getUserInfoResult.ContainsError);
            Assert.Equal("invalid_token", getUserInfoResult.Error.Error);
            Assert.Equal("not a valid resource owner token", getUserInfoResult.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Json_Is_Returned()
        {            InitializeFakeObjects();

                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client", "client"),
                    TokenRequest.FromPassword("administrator", "password", new []{"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken).ConfigureAwait(false);

                        Assert.NotNull(getUserInfoResult);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jws_Is_Returned()
        {            InitializeFakeObjects();

                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client_userinfo_sig_rs256", "client_userinfo_sig_rs256"),
                    TokenRequest.FromPassword("administrator", "password", new []{"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken).ConfigureAwait(false);

                        Assert.NotNull(getUserInfoResult);
            Assert.NotNull(getUserInfoResult.JwtToken);
        }

        [Fact]
        public async Task When_Pass_Access_Token_Then_Jwe_Is_Returned()
        {            InitializeFakeObjects();

                        var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("client_userinfo_enc_rsa15", "client_userinfo_enc_rsa15"),
                    TokenRequest.FromPassword("administrator", "password", new []{"scim"}),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(baseUrl + "/.well-known/openid-configuration").ConfigureAwait(false);
            var getUserInfoResult = await _userInfoClient.Resolve(baseUrl + "/.well-known/openid-configuration", result.Content.AccessToken).ConfigureAwait(false);

                        Assert.NotNull(getUserInfoResult);
            Assert.NotNull(getUserInfoResult.JwtToken);
        }

        private void InitializeFakeObjects()
        {
            var getDiscoveryOperation = new GetDiscoveryOperation(_server.Client);
            _userInfoClient = new UserInfoClient(_server.Client, getDiscoveryOperation);
        }
    }
}
