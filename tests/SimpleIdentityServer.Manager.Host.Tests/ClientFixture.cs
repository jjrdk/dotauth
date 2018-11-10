using Moq;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Manager.Client.Clients;
using SimpleIdentityServer.Manager.Client.Configuration;
using SimpleIdentityServer.Manager.Common.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class ClientFixture : IClassFixture<TestManagerServerFixture>
    {
        private TestManagerServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IOpenIdClients _openidClients;

        public ClientFixture(TestManagerServerFixture server)
        {
            _server = server;
        }

        #region Errors

        #region Add

        [Fact]
        public async Task When_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {

            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_redirect_uri", result.Error.Error);
            Assert.Equal("the parameter request_uris is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_User_And_Redirect_Uri_Is_Not_Valid_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                RedirectUris = new List<string> { "invalid_uris" }
            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_redirect_uri", result.Error.Error);
            Assert.Equal("the redirect_uri invalid_uris is not well formed", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_User_And_Redirect_Uri_Contains_Fragment_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                RedirectUris = new List<string> { "http://localhost#fragment" }
            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_redirect_uri", result.Error.Error);
            Assert.Equal("the redirect_uri http://localhost#fragment cannot contains fragment", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_User_And_Redirect_Uri_Is_Not_Valid_And_Client_Is_Native_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                RedirectUris = new List<string> { "invalid_redirect_uri" },
                ApplicationType = ApplicationTypes.native
            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_redirect_uri", result.Error.Error);
            Assert.Equal("the redirect_uri invalid_redirect_uri is not well formed", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_User_And_And_Logo_Uri_Is_Not_Valid_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                RedirectUris = new List<string> { "http://localhost" },
                LogoUri = "logo_uri"
            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_client_metadata", result.Error.Error);
            Assert.Equal("the parameter logo_uri is not correct", result.Error.ErrorDescription);
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Update_And_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var result = await _openidClients.ResolveUpdate(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new UpdateClientRequest
            {

            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_parameter", result.Error.Error);
            Assert.Equal("the parameter client_id is missing", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Add_Pass_Invalid_Scopes_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addClientResult = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                ClientUri = "http://clienturi.com",
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                RedirectUris = new List<string> { "http://localhost" },
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                LogoUri = "http://logouri.com"
            }, null);

            // ACT
            var result = await _openidClients.ResolveUpdate(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new UpdateClientRequest
            {
                ClientId = addClientResult.Content.ClientId,
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                RedirectUris = new List<string> { "http://localhost" },
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                AllowedScopes = new List<string>
                {
                    "not_valid"
                }
            }, null);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal("invalid_parameter", result.Error.Error);
            Assert.Equal("the scopes 'not_valid' don't exist", result.Error.ErrorDescription);
        }

        #endregion

        #region Get

        [Fact]
        public async Task When_Get_Unknown_Client_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var newClient = await _openidClients.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "unknown_client");

            // ASSERTS
            Assert.NotNull(newClient);
            Assert.True(newClient.ContainsError);
            Assert.Equal("invalid_request", newClient.Error.Error);
            Assert.Equal("the client 'unknown_client' doesn't exist", newClient.Error.ErrorDescription);
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_An_Unknown_Client_Then_Error_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);

            // ACT
            var newClient = await _openidClients.ResolveDelete(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), "unknown_client");

            // ASSERTS
            Assert.NotNull(newClient);
            Assert.True(newClient.ContainsError);
            Assert.Equal("invalid_request", newClient.Error.Error);
            Assert.Equal("the client 'unknown_client' doesn't exist", newClient.Error.ErrorDescription);
        }

        #endregion

        #endregion

        #region Happy path

        #region Add

        [Fact]
        public async Task When_Add_Client_Then_Informations_Are_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                IdTokenSignedResponseAlg = "RS256",
                IdTokenEncryptedResponseAlg = "RSA1_5",
                IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                UserInfoSignedResponseAlg = "RS256",
                UserInfoEncryptedResponseAlg = "RSA1_5",
                UserInfoEncryptedResponseEnc = "A128CBC-HS256",
                RequestObjectSigningAlg = "RS256",
                RequestObjectEncryptionAlg = "RSA1_5",
                RequestObjectEncryptionEnc = "A128CBC-HS256",
                TokenEndPointAuthMethod = "client_secret_post",
                InitiateLoginUri = "https://initloginuri",
                ClientUri = "http://clienturi.com",
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code,
                    ResponseType.id_token,
                    ResponseType.token
                },
                RedirectUris = new List<string> { "http://localhost" },
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                LogoUri = "http://logouri.com"
            }, null);
            var newClient = await _openidClients.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), result.Content.ClientId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(newClient.ContainsError);
            Assert.Equal("web", newClient.Content.ApplicationType);
            Assert.Equal("client_name", newClient.Content.ClientName);
            Assert.Equal("http://clienturi.com", newClient.Content.ClientUri);
            Assert.Equal("http://logouri.com", newClient.Content.LogoUri);
            Assert.Equal(10, newClient.Content.DefaultMaxAge);
            Assert.Equal("sms", newClient.Content.DefaultAcrValues);
            Assert.Equal(1, newClient.Content.Contacts.Count());
            Assert.Equal(1, newClient.Content.RedirectUris.Count());
            Assert.Equal(1, newClient.Content.PostLogoutRedirectUris.Count());
            Assert.Equal(3, newClient.Content.GrantTypes.Count());
            Assert.Equal(3, newClient.Content.ResponseTypes.Count());
        }

        #endregion

        #region Update

        [Fact]
        public async Task When_Update_Client_Then_Information_Are_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addClientResult = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                ClientUri = "http://clienturi.com",
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                RedirectUris = new List<string> { "http://localhost" },
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                LogoUri = "http://logouri.com"
            }, null);

            // ACT
            var result = await _openidClients.ResolveUpdate(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new UpdateClientRequest
            {
                ClientId = addClientResult.Content.ClientId,
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                RedirectUris = new List<string> { "http://localhost" },
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                AllowedScopes = new List<string>
                {
                    "openid"
                }
            }, null);
            var newClient = await _openidClients.ResolveGet(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), addClientResult.Content.ClientId);

            // ASSERTS
            Assert.NotNull(result);
            Assert.False(result.ContainsError);
            Assert.Equal(1, newClient.Content.PostLogoutRedirectUris.Count());
            Assert.Equal(1, newClient.Content.RedirectUris.Count());
            Assert.Equal(3, newClient.Content.GrantTypes.Count());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task When_Delete_Client_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var addClientResult = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                ClientUri = "http://clienturi.com",
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                RedirectUris = new List<string> { "http://localhost" },
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                LogoUri = "http://logouri.com"
            }, null);

            // ACT
            var deleteResult = await _openidClients.ResolveDelete(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), addClientResult.Content.ClientId);

            // ASSERTS
            Assert.NotNull(deleteResult);
            Assert.False(deleteResult.ContainsError);
        }

        #endregion

        #endregion

        [Fact]
        public async Task When_Search_One_Client_Then_One_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var result = await _openidClients.ResolveAdd(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new AddClientRequest
            {
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                IdTokenSignedResponseAlg = "RS256",
                IdTokenEncryptedResponseAlg = "RSA1_5",
                IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                UserInfoSignedResponseAlg = "RS256",
                UserInfoEncryptedResponseAlg = "RSA1_5",
                UserInfoEncryptedResponseEnc = "A128CBC-HS256",
                RequestObjectSigningAlg = "RS256",
                RequestObjectEncryptionAlg = "RSA1_5",
                RequestObjectEncryptionEnc = "A128CBC-HS256",
                TokenEndPointAuthMethod = "client_secret_post",
                InitiateLoginUri = "https://initloginuri",
                ClientUri = "http://clienturi.com",
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code,
                    ResponseType.id_token,
                    ResponseType.token
                },
                RedirectUris = new List<string> { "http://localhost" },
                PostLogoutRedirectUris = new List<string> { "http://localhost/callback" },
                LogoUri = "http://logouri.com"
            }, null);

            // ACT
            var searchResult = await _openidClients.ResolveSearch(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new SearchClientsRequest
            {
                StartIndex = 0,
                NbResults = 1
            }, null);

            // ASSERTS
            Assert.NotNull(searchResult);
            Assert.False(searchResult.ContainsError);
            Assert.Equal(1, searchResult.Content.Content.Count());
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var addClientOperation = new AddClientOperation(_httpClientFactoryStub.Object);
            var deleteClientOperation = new DeleteClientOperation(_httpClientFactoryStub.Object);
            var getAllClientsOperation = new GetAllClientsOperation(_httpClientFactoryStub.Object);
            var getClientOperation = new GetClientOperation(_httpClientFactoryStub.Object);
            var searchClientOperation = new SearchClientOperation(_httpClientFactoryStub.Object);
            var updateClientOperation = new UpdateClientOperation(_httpClientFactoryStub.Object);
            var configurationClient = new ConfigurationClient(new GetConfigurationOperation(_httpClientFactoryStub.Object));
            _openidClients = new OpenIdClients(
                addClientOperation, updateClientOperation, getAllClientsOperation, deleteClientOperation, getClientOperation,
                searchClientOperation, configurationClient);
        }
    }
}
