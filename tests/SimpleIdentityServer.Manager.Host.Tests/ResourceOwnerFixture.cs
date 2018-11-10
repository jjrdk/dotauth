using Moq;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Client.ResourceOwners;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class ResourceOwnerFixture : IClassFixture<TestManagerServerFixture>
    {
        private TestManagerServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IResourceOwnerClient _resourceOwnerClient;

        public ResourceOwnerFixture(TestManagerServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Get resource owner

        [Fact]
        public async Task When_Trying_To_Get_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "invalid_login", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Add resource owner

        [Fact]
        public async Task When_Pass_No_Login_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.AddResourceOwnerRequest(), null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter login is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_No_Password_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.AddResourceOwnerRequest
            {
                Subject = "subject"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter password is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Login_Already_Exists_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.AddResourceOwnerRequest
            {
                Subject = "administrator",
                Password = "password"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("unhandled_exception", result.Error.Error);
            Assert.Equal("a resource owner with same credentials already exists", result.Error.ErrorDescription);
        }

        #endregion

        #region Update claims

        [Fact]
        public async Task When_Update_Claims_And_No_Login_Is_Passwed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdateClaims(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerClaimsRequest(), null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter login is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_And_Resource_Owner_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdateClaims(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerClaimsRequest
            {
                Login = "invalid_login"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_parameter", result.Error.Error);
            Assert.Equal("the resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Update password

        [Fact]
        public async Task When_Update_Password_And_No_Login_Is_Passed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdatePassword(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerPasswordRequest(), null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter login is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_No_Password_Is_Passwed_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdatePassword(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerPasswordRequest
            {
                Login = "login"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the parameter password is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_Resource_Owner_Doesnt_Exist_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdatePassword(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerPasswordRequest
            {
                Login = "invalid_login",
                Password = "password"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_parameter", result.Error.Error);
            Assert.Equal("the resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Delete resource owner

        [Fact]
        public async Task When_Delete_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveDelete(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "invalid_login", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_request", result.Error.Error);
            Assert.Equal("the resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy paths

        #region Update claims

        [Fact]
        public async Task When_Update_Claims_Then_ResourceOwner_Is_Updated()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdateClaims(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerClaimsRequest
            {
                Login = "administrator",
                Claims = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>
                {
                    new System.Collections.Generic.KeyValuePair<string, string>("role", "role"),
                    new System.Collections.Generic.KeyValuePair<string, string>("not_valid", "not_valid")
                }
            }, null);
            var resourceOwner = await _resourceOwnerClient.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "administrator", null);

            // ASSERTS
            Assert.NotNull(resourceOwner);
            Assert.False(resourceOwner.ContainsError);
            Assert.Equal("role", resourceOwner.Content.Claims.First(c => c.Key == "role").Value);
        }

        #endregion

        #region Update password

        [Fact]
        public async Task When_Update_Password_Then_ResourceOner_Is_Updated()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveUpdatePassword(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.UpdateResourceOwnerPasswordRequest
            {
                Login = "administrator",
                Password = "pass"
            }, null);
            var resourceOwner = await _resourceOwnerClient.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "administrator", null);

            // ASSERTS
            Assert.NotNull(resourceOwner);
            Assert.Equal(PasswordHelper.ComputeHash("pass"), resourceOwner.Content.Password);
        }

        #endregion

        #region Search

        [Fact]
        public async Task When_Search_Resource_Owners_Then_One_Resource_Owner_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveSearch(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.SearchResourceOwnersRequest
            {
                StartIndex = 0,
                NbResults = 1
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.Content.Content.Count());
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Get all

        [Fact]
        public async Task When_Get_All_ResourceOwners_Then_All_Resource_Owners_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var resourceOwners = await _resourceOwnerClient.ResolveGetAll(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), null);

            // ASSERT
            Assert.NotNull(resourceOwners);
            Assert.False(resourceOwners.ContainsError);
            Assert.True(resourceOwners.Content.Any());
        }

        #endregion

        #region Add

        [Fact]
        public async Task When_Add_Resource_Owner_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.AddResourceOwnerRequest
            {
                Subject = "login",
                Password = "password"
            }, null);

            // ASSERT
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_ResourceOwner_Then_ResourceOwner_Doesnt_Exist()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _resourceOwnerClient.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Requests.AddResourceOwnerRequest
            {
                Subject = "login1",
                Password = "password"
            }, null);
            var remove = await _resourceOwnerClient.ResolveDelete(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "login1", null);

            // ASSERT
            Assert.NotNull(remove);
            Assert.False(remove.ContainsError);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var addResourceOwnerOperation = new AddResourceOwnerOperation(_httpClientFactoryStub.Object);
            var deleteResourceOwnerOperation = new DeleteResourceOwnerOperation(_httpClientFactoryStub.Object);
            var getAllResourceOwnersOperation = new GetAllResourceOwnersOperation(_httpClientFactoryStub.Object);
            var getResourceOwnerOperation = new GetResourceOwnerOperation(_httpClientFactoryStub.Object);
            var updateResourceOwnerClaimsOperation = new UpdateResourceOwnerClaimsOperation(_httpClientFactoryStub.Object);
            var updateResourceOwnerPasswordOperation = new UpdateResourceOwnerPasswordOperation(_httpClientFactoryStub.Object);
            var getConfigurationOperation = new GetConfigurationOperation(_httpClientFactoryStub.Object);
            var configurationClient = new ConfigurationClient(getConfigurationOperation);
            var searchResourceOwnersOperation = new SearchResourceOwnersOperation(_httpClientFactoryStub.Object);
            _resourceOwnerClient = new ResourceOwnerClient(addResourceOwnerOperation, deleteResourceOwnerOperation,
                getAllResourceOwnersOperation, getResourceOwnerOperation, updateResourceOwnerClaimsOperation, updateResourceOwnerPasswordOperation, configurationClient, searchResourceOwnersOperation);
        }
    }
}
