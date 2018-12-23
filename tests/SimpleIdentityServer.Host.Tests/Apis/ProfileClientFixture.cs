using SimpleIdentityServer.Client.Operations;
using SimpleIdentityServer.UserManagement.Client;
using SimpleIdentityServer.UserManagement.Client.Operations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Host.Tests.Apis
{
    using Client;
    using Core.Errors;
    using UserManagement.Common.Requests;

    public class ProfileClientFixture : IClassFixture<TestOauthServerFixture>
    {
        private const string baseUrl = "http://localhost:5000";
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
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration")
                .ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {

            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter user_id is missing", result.Error.ErrorDescription);
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
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "currentSubject", new LinkProfileRequest
            {
                UserId = "user_id"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter issuer is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Link_Profile_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "currentSubject";
            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", currentSubject, new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, currentSubject), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unlink_Profile_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "currentSubject";
            var result = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "externalSubject", currentSubject, grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, currentSubject), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Unlink_Profile_And_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "invalid_external_subject", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("internal_error", result.Error.Error);
            Assert.Equal("not authorized to remove the profile", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Get_Profiles_And_User_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var grantedToken = await new TokenClient(
                    TokenCredentials.FromClientCredentials("stateless_client", "stateless_client"),
                    TokenRequest.FromScopes("manage_profile"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var currentSubject = "notvalid";
            var result = await _profileClient.GetProfiles(baseUrl + "/profiles", currentSubject, grantedToken.Content.AccessToken).ConfigureAwait(false);

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
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);

            var result = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
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
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            var unlinkResult = await _profileClient.UnlinkProfile(baseUrl + "/profiles", "user_id", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

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
                .ResolveAsync($"{baseUrl}/.well-known/openid-configuration").ConfigureAwait(false);
            var linkResult = await _profileClient.LinkProfile(baseUrl + "/profiles", "administrator", new LinkProfileRequest
            {
                UserId = "user_id",
                Issuer = "issuer"
            }, grantedToken.Content.AccessToken).ConfigureAwait(false);

            var getProfilesResult = await _profileClient.GetProfiles(baseUrl + "/profiles", "administrator", grantedToken.Content.AccessToken).ConfigureAwait(false);

            Assert.False(getProfilesResult.ContainsError);
            Assert.True(getProfilesResult.Content.Any());

        }

        private void InitializeFakeObjects()
        {
            var linkProfileOperation = new LinkProfileOperation(_server.Client);
            var unlinkProfileOperation = new UnlinkProfileOperation(_server.Client);
            var getProfilesOperation = new GetProfilesOperation(_server.Client);
            _profileClient = new ProfileClient(linkProfileOperation, unlinkProfileOperation, getProfilesOperation);
        }
    }
}
