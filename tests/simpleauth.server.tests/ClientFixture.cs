namespace SimpleAuth.Server.Tests
{
    using Shared;
    using Shared.Models;
    using Shared.Requests;
    using SimpleAuth.Manager.Client;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ClientFixture
    {
        private const string OpenidmanagerConfiguration = "http://localhost:5000/.well-known/openid-configuration";
        private readonly TestManagerServerFixture _server;
        private readonly ManagementClient _openidClients;

        public ClientFixture()
        {
            _server = new TestManagerServerFixture();
            _openidClients = ManagementClient.Create(_server.Client, new Uri(OpenidmanagerConfiguration)).Result;
        }

        [Fact]
        public async Task When_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            var result = await _openidClients.AddClient(new Client()).ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Error.Title);
        }

        [Fact]
        public async Task When_Add_User_And_Redirect_Uri_Contains_Fragment_Then_Error_Is_Returned()
        {
            var result = await _openidClients.AddClient(
                    new Client
                    {
                        JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                        AllowedScopes = new[] { "openid" },
                        ClientId = "test",
                        ClientName = "name",
                        RedirectionUrls = new[] { new Uri("http://localhost#fragment") },
                        RequestUris = new[] { new Uri("https://localhost") }
                    })
                .ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal("invalid_redirect_uri", result.Error.Title);
            Assert.Equal("The redirect_uri http://localhost/#fragment cannot contain fragment", result.Error.Detail);
        }

        [Fact]
        public async Task When_Update_And_Pass_No_Parameter_Then_Error_Is_Returned()
        {
            var result = await _openidClients.UpdateClient(new Client()).ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Title);
            Assert.Equal(ErrorDescriptions.RequestIsNotValid, result.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Add_Pass_Invalid_Scopes_Then_Error_Is_Returned()
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid().ToString("N"),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = new[] { "openid" },
                RequestUris = new[] { new Uri("https://localhost"), },
                ApplicationType = ApplicationTypes.Web,
                ClientName = "client_name",
                ClientUri = new Uri("http://clienturi.com"),
                Contacts = new[] { "contact" },
                DefaultAcrValues = "sms",
                //DefaultMaxAge = 10,
                GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                //LogoUri = new Uri("http://logouri.com")
            };
            var addClientResult = await _openidClients.AddClient(client).ConfigureAwait(false);
            client = addClientResult.Content;
            client.AllowedScopes = new[] { "not_valid" };
            var result = await _openidClients.UpdateClient(client).ConfigureAwait(false);

            Assert.True(result.HasError);
            Assert.Equal(ErrorCodes.InvalidScope, result.Error.Title);
            Assert.Equal("Unknown scopes: not_valid", result.Error.Detail);
        }

        [Fact]
        public async Task When_Get_Unknown_Client_Then_Error_Is_Returned()
        {
            var newClient = await _openidClients.GetClient("unknown_client").ConfigureAwait(false);

            Assert.True(newClient.HasError);
            Assert.Equal(ErrorCodes.InvalidRequest, newClient.Error.Title);
            Assert.Equal(ErrorDescriptions.TheClientDoesntExist, newClient.Error.Detail);
        }

        [Fact]
        public async Task When_Delete_An_Unknown_Client_Then_Error_Is_Returned()
        {
            var newClient = await _openidClients.DeleteClient("unknown_client").ConfigureAwait(false);

            Assert.True(newClient.HasError);
        }

        [Fact]
        public async Task When_Add_Client_Then_Informations_Are_Correct()
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid().ToString("N"),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = new[] { "openid" },
                ApplicationType = ApplicationTypes.Web,
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
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                InitiateLoginUri = new Uri("https://initloginuri"),
                ClientUri = new Uri("http://clienturi.com"),
                Contacts = new[] { "contact" },
                DefaultAcrValues = "sms",
                //DefaultMaxAge = 10,
                GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken },
                ResponseTypes = new[] { ResponseTypeNames.Code, ResponseTypeNames.IdToken, ResponseTypeNames.Token },
                RequestUris = new[] { new Uri("https://localhost"), },
                RedirectionUrls = new[] { new Uri("http://localhost"), },
                PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback"), },
                //LogoUri = new Uri("http://logouri.com")
            };
            var result = await _openidClients.AddClient(client).ConfigureAwait(false);

            Assert.False(result.HasError, result.Error?.Detail);

            var newClient = await _openidClients.GetClient(result.Content.ClientId).ConfigureAwait(false);

            Assert.False(newClient.HasError);
            Assert.Equal(ApplicationTypes.Web, newClient.Content.ApplicationType);
            Assert.Equal("client_name", newClient.Content.ClientName);
            Assert.Equal(new Uri("http://clienturi.com"), newClient.Content.ClientUri);
            //Assert.Equal(new Uri("http://logouri.com"), newClient.Content.LogoUri);
            //Assert.Equal(10, newClient.Content.DefaultMaxAge);
            Assert.Equal("sms", newClient.Content.DefaultAcrValues);
            Assert.Single(newClient.Content.Contacts);
            Assert.Single(newClient.Content.RedirectionUrls);
            Assert.Single(newClient.Content.PostLogoutRedirectUris);
            Assert.Equal(3, newClient.Content.GrantTypes.Length);
            Assert.Equal(3, newClient.Content.ResponseTypes.Length);
        }

        [Fact]
        public async Task When_Update_Client_Then_Information_Are_Correct()
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid().ToString("N"),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = new[] { "openid" },
                ApplicationType = ApplicationTypes.Web,
                ClientName = "client_name",
                ClientUri = new Uri("http://clienturi.com"),
                Contacts = new[] { "contact" },
                DefaultAcrValues = "sms",
                // DefaultMaxAge = 10,
                GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken },
                RequestUris = new[] { new Uri("https://localhost") },
                RedirectionUrls = new[] { new Uri("http://localhost") },
                PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                //LogoUri = new Uri("http://logouri.com")
            };

            var addClientResult = await _openidClients.AddClient(client).ConfigureAwait(false);
            client = addClientResult.Content;
            client.PostLogoutRedirectUris = new[]
            {
                new Uri("http://localhost/callback"), new Uri("http://localhost/callback2"),
            };
            client.GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, };
            var result = await _openidClients.UpdateClient(client).ConfigureAwait(false);
            var newClient = await _openidClients.GetClient(result.Content.ClientId).ConfigureAwait(false);

            Assert.False(result.HasError);
            Assert.Equal(2, newClient.Content.PostLogoutRedirectUris.Length);
            Assert.Single(newClient.Content.RedirectionUrls);
            Assert.Equal(2, newClient.Content.GrantTypes.Length);
        }

        [Fact]
        public async Task When_Delete_Client_Then_Ok_Is_Returned()
        {
            var addClientResult = await _openidClients.AddClient(
                    new Client
                    {
                        ClientId = Guid.NewGuid().ToString("N"),
                        JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                        AllowedScopes = new[] { "openid" },
                        ApplicationType = ApplicationTypes.Web,
                        ClientName = "client_name",
                        ClientUri = new Uri("http://clienturi.com"),
                        Contacts = new[] { "contact" },
                        DefaultAcrValues = "sms",
                        //DefaultMaxAge = 10,
                        GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken },
                        RequestUris = new[] { new Uri("https://localhost"), },
                        RedirectionUrls = new[] { new Uri("http://localhost") },
                        PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                        //LogoUri = new Uri("http://logouri.com")
                    })
                .ConfigureAwait(false);

            var deleteResult =
                await _openidClients.DeleteClient(addClientResult.Content.ClientId).ConfigureAwait(false);

            Assert.False(deleteResult.HasError);
        }

        [Fact]
        public async Task When_Search_One_Client_Then_One_Client_Is_Returned()
        {
            var result = await _openidClients.AddClient(
                    new Client
                    {
                        ClientId = Guid.NewGuid().ToString("N"),
                        AllowedScopes = new[] { "openid" },
                        RequestUris = new[] { new Uri("https://localhost"), },
                        ApplicationType = ApplicationTypes.Web,
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
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                        InitiateLoginUri = new Uri("https://initloginuri"),
                        ClientUri = new Uri("http://clienturi.com"),
                        Contacts = new[] { "contact" },
                        DefaultAcrValues = "sms",
                        //DefaultMaxAge = 10,
                        GrantTypes = new[] { GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken },
                        ResponseTypes = new[]
                            {
                                ResponseTypeNames.Code, ResponseTypeNames.IdToken, ResponseTypeNames.Token
                            },
                        JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                        RedirectionUrls = new[] { new Uri("http://localhost") },
                        PostLogoutRedirectUris = new[] { new Uri("http://localhost/callback") },
                        //LogoUri = new Uri("http://logouri.com")
                    })
                .ConfigureAwait(false);

            var searchResult = await _openidClients.SearchClients(
                    new SearchClientsRequest { StartIndex = 0, NbResults = 1 })
                .ConfigureAwait(false);

            Assert.False(searchResult.HasError);
            Assert.Single(searchResult.Content.Content);
        }
    }
}
