namespace SimpleAuth.Server.Tests.Apis
{
    using Client;
    using Client.Operations;
    using Errors;
    using Manager.Client;
    using Shared.Requests;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using TokenRequest = Client.TokenRequest;

    public class ProfileClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string BaseUrl = "http://localhost:5000";
        private readonly TestOauthServerFixture _server;
        private IProfileClient _profileClient;

        public ProfileClientFixture(TestOauthServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Link_Profile_And_No_UserId_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(BaseUrl + "/profiles",
                    "user",
                    new LinkProfileRequest
                    {
                    },
                    grantedToken.Content.AccessToken)
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, result.Error.Error);
            Assert.Equal("the parameter UserId is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Link_Profile_And_No_Issuer_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(BaseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {
                UserId = "user_id",
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, result.Error.Error);
            Assert.Equal("the parameter Issuer is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Link_Profile_And_User_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "currentSubject";
            var result = await _profileClient.LinkProfile(BaseUrl + "/profiles", currentSubject, new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, currentSubject), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unlink_Profile_And_User_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "currentSubject";
            var result = await _profileClient.UnlinkProfile(BaseUrl + "/profiles", "externalSubject", currentSubject, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, currentSubject), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unlink_Profile_And_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.UnlinkProfile(BaseUrl + "/profiles", "invalid_external_subject", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("not authorized to remove the profile", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Get_Profiles_And_User_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "notvalid";
            var result = await _profileClient.GetProfiles(BaseUrl + "/profiles", currentSubject, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, currentSubject), result.Error.ErrorDescription);
        }


        [Fact]
        public async Task When_Link_Profile_Then_Ok_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(BaseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id_1",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Unlink_Profile_Then_Ok_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(BaseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            var unlinkResult = await _profileClient.UnlinkProfile(BaseUrl + "/profiles", "user_id", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.False(unlinkResult.ContainsError);
        }

        [Fact]
        public async Task When_Get_Profiles_Then_List_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{BaseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(BaseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            var getProfilesResult = await _profileClient.GetProfiles(BaseUrl + "/profiles", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.False(getProfilesResult.ContainsError);
            Assert.True(getProfilesResult.Content.Any());

        }

        private void InitializeFakeObjects()
        {
            _profileClient = new ProfileClient(_server.Client);
        }
    }
}
