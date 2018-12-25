using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    using Client.Clients;
    using Client.Configuration;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using System.Linq;
    using SimpleAuth.Errors;

    public class ClientFixture //: IClassFixture<TestManagerServerFixture>
    {
        private const string OpenidmanagerConfiguration = "http://localhost:5000/.well-known/openid-configuration";
        private readonly TestManagerServerFixture _server;
        private IOpenIdClients _openidClients;

        public ClientFixture()
        {
            _server = new TestManagerServerFixture();
        }

        [Fact]
        public async Task When_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {

                    },
                    null)
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
        }

        [Fact]
        public async Task When_Add_User_And_Redirect_Uri_Contains_Fragment_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {
                        AllowedScopes = new[] { new Scope { Name = "openid" } },
                        ClientId = "test",
                        ClientName = "name",
                        RedirectionUrls = new[] { new Uri("http://localhost#fragment") },
                        RequestUris = new[] { new Uri("https://localhost") }
                    },
                    null)
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal("invalid_redirect_uri", result.Error.Error);
            Assert.Equal("The redirect_uri http://localhost/#fragment cannot contain fragment",
                result.Error.ErrorDescription);
        }

        //[Fact]
        //public async Task When_Add_User_And_Redirect_Uri_Is_Not_Valid_And_Client_Is_Native_Then_Error_Is_Returned()
        //{
        //    InitializeFakeObjects();

        //    var result = await _openidClients.ResolveAdd(
        //            new Uri("http://localhost:5000/.well-known/openid-configuration"),
        //            new Client
        //            {
        //                RedirectionUrls = new List<string> { "invalid_redirect_uri" },
        //                ApplicationType = ApplicationTypes.native
        //            },
        //            null)
        //        .ConfigureAwait(false);

        //    Assert.NotNull(result);
        //    Assert.True(result.ContainsError);
        //    Assert.Equal("invalid_redirect_uri", result.Error.Error);
        //    Assert.Equal("the redirect_uri invalid_redirect_uri is not well formed", result.Error.ErrorDescription);
        //}

        //[Fact]
        //public async Task When_Add_User_And_And_Logo_Uri_Is_Not_Valid_Then_Error_Is_Returned()
        //{
        //    InitializeFakeObjects();

        //    var result = await _openidClients.ResolveAdd(
        //            new Uri("http://localhost:5000/.well-known/openid-configuration"),
        //            new Client
        //            {
        //                RedirectUris = new List<string> { "http://localhost" },
        //                LogoUri = "logo_uri"
        //            },
        //            null)
        //        .ConfigureAwait(false);

        //    Assert.NotNull(result);
        //    Assert.True(result.ContainsError);
        //    Assert.Equal("invalid_client_metadata", result.Error.Error);
        //    Assert.Equal("the parameter logo_uri is not correct", result.Error.ErrorDescription);
        //}

        [Fact]
        public async Task When_Update_And_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _openidClients.ResolveUpdate(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {

                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(ErrorDescriptions.RequestIsNotValid, result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Add_Pass_Invalid_Scopes_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();
            var client = new Client
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                RequestUris = new[] { new Uri("https://localhost"), },
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                ClientUri = new Uri("http://clienturi.com"),
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
                RedirectionUrls = new[] { new Uri("http://localhost") },
                PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                LogoUri = new Uri("http://logouri.com")
            };
            var addClientResult = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    client,
                    null)
                .ConfigureAwait(false);
            client = addClientResult.Content;
            client.AllowedScopes = new[] { new Scope { Name = "not_valid" } };
            var result = await _openidClients.ResolveUpdate(
                    new Uri(OpenidmanagerConfiguration),
                    client,
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.InvalidScope, result.Error.Error);
            Assert.Equal("Unknown scopes: not_valid", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Get_Unknown_Client_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var newClient = await _openidClients
                .ResolveGet(new Uri(OpenidmanagerConfiguration), "unknown_client")
                .ConfigureAwait(false);

            Assert.NotNull(newClient);
            Assert.True(newClient.ContainsError);
            Assert.Equal("invalid_request", newClient.Error.Error);
            Assert.Equal(ErrorDescriptions.TheClientDoesntExist, newClient.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Delete_An_Unknown_Client_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var newClient = await _openidClients
                .ResolveDelete(new Uri(OpenidmanagerConfiguration),
                    "unknown_client")
                .ConfigureAwait(false);

            Assert.True(newClient.ContainsError);
            //Assert.Equal("invalid_request", newClient.Error.Error);
            //Assert.Equal("the client 'unknown_client' doesn't exist", newClient.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Add_Client_Then_Informations_Are_Correct()
        {
            InitializeFakeObjects();
            var result = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {
                        AllowedScopes = new[] { new Scope { Name = "openid" } },
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
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        InitiateLoginUri = new Uri("https://initloginuri"),
                        ClientUri = new Uri("http://clienturi.com"),
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
                        RequestUris = new[] { new Uri("https://localhost"), },
                        RedirectionUrls = new[] { new Uri("http://localhost"), },
                        PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback"), },
                        LogoUri = new Uri("http://logouri.com")
                    },
                    null)
                .ConfigureAwait(false);

            Assert.False(result.ContainsError, result.Error?.ErrorDescription);

            var newClient = await _openidClients
                .ResolveGet(new Uri(OpenidmanagerConfiguration),
                    result.Content.ClientId)
                .ConfigureAwait(false);

            Assert.False(newClient.ContainsError);
            Assert.Equal(ApplicationTypes.web, newClient.Content.ApplicationType);
            Assert.Equal("client_name", newClient.Content.ClientName);
            Assert.Equal(new Uri("http://clienturi.com"), newClient.Content.ClientUri);
            Assert.Equal(new Uri("http://logouri.com"), newClient.Content.LogoUri);
            Assert.Equal(10, newClient.Content.DefaultMaxAge);
            Assert.Equal("sms", newClient.Content.DefaultAcrValues);
            Assert.Single(newClient.Content.Contacts);
            Assert.Single(newClient.Content.RedirectionUrls);
            Assert.Single(newClient.Content.PostLogoutRedirectUris);
            Assert.Equal(3, newClient.Content.GrantTypes.Count());
            Assert.Equal(3, newClient.Content.ResponseTypes.Count());
        }

        [Fact]
        public async Task When_Update_Client_Then_Information_Are_Correct()
        {
            InitializeFakeObjects();
            var client = new Client
            {
                AllowedScopes = new[] { new Scope { Name = "openid" } },
                ApplicationType = ApplicationTypes.web,
                ClientName = "client_name",
                ClientUri = new Uri("http://clienturi.com"),
                Contacts = new List<string>
                {
                    "contact"
                },
                DefaultAcrValues = "sms",
                DefaultMaxAge = 10,
                GrantTypes = new[]
                {
                    GrantType.authorization_code,
                    GrantType.@implicit,
                    GrantType.refresh_token
                },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                LogoUri = new Uri("http://logouri.com")
            };

            var addClientResult = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    client,
                    null)
                .ConfigureAwait(false);
            client = addClientResult.Content;
            client.PostLogoutRedirectUris = new[]
            {
                new Uri("http://localhost/callback"),
                new Uri("http://localhost/callback2"),
            };
            client.GrantTypes = new[]
            {
                GrantType.authorization_code,
                GrantType.@implicit,
            };
            var result = await _openidClients.ResolveUpdate(
                    new Uri(OpenidmanagerConfiguration),
                    client,
                    null)
                .ConfigureAwait(false);
            var newClient = await _openidClients
                .ResolveGet(new Uri(OpenidmanagerConfiguration),
                    addClientResult.Content.ClientId)
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
            Assert.Equal(2, newClient.Content.PostLogoutRedirectUris.Count);
            Assert.Single(newClient.Content.RedirectionUrls);
            Assert.Equal(2, newClient.Content.GrantTypes.Count);
        }

        [Fact]
        public async Task When_Delete_Client_Then_Ok_Is_Returned()
        {
            InitializeFakeObjects();
            var addClientResult = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {
                        AllowedScopes = new[] { new Scope { Name = "openid" } },
                        ApplicationType = ApplicationTypes.web,
                        ClientName = "client_name",
                        ClientUri = new Uri("http://clienturi.com"),
                        Contacts = new[]
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
                        RequestUris = new[] { new Uri("https://localhost"), },
                        RedirectionUrls = new[] { new Uri("http://localhost") },
                        PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                        LogoUri = new Uri("http://logouri.com")
                    },
                    null)
                .ConfigureAwait(false);

            var deleteResult = await _openidClients
                .ResolveDelete(new Uri(OpenidmanagerConfiguration),
                    addClientResult.Content.ClientId)
                .ConfigureAwait(false);

            Assert.NotNull(deleteResult);
            Assert.False(deleteResult.ContainsError);
        }

        [Fact]
        public async Task When_Search_One_Client_Then_One_Client_Is_Returned()
        {
            InitializeFakeObjects();
            var result = await _openidClients.ResolveAdd(
                    new Uri(OpenidmanagerConfiguration),
                    new Client
                    {
                        AllowedScopes = new[] { new Scope { Name = "openid" } },
                        RequestUris = new[] { new Uri("https://localhost"), },
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
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        InitiateLoginUri = new Uri("https://initloginuri"),
                        ClientUri = new Uri("http://clienturi.com"),
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
                        RedirectionUrls = new[] { new Uri("http://localhost") },
                        PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                        LogoUri = new Uri("http://logouri.com")
                    },
                    null)
                .ConfigureAwait(false);

            var searchResult = await _openidClients.ResolveSearch(
                    new Uri(OpenidmanagerConfiguration),
                    new SearchClientsRequest
                    {
                        StartIndex = 0,
                        NbResults = 1
                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(searchResult);
            Assert.False(searchResult.ContainsError);
            Assert.Single(searchResult.Content.Content);
        }

        private void InitializeFakeObjects()
        {
            var deleteClientOperation = new DeleteClientOperation(_server.Client);
            var getAllClientsOperation = new GetAllClientsOperation(_server.Client);
            var getClientOperation = new GetClientOperation(_server.Client);
            var searchClientOperation = new SearchClientOperation(_server.Client);
            _openidClients = new OpenIdClients(
                _server.Client,
                getAllClientsOperation,
                deleteClientOperation,
                getClientOperation,
                searchClientOperation);
        }
    }
}
