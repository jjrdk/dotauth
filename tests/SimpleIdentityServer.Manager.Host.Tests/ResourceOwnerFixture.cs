namespace SimpleIdentityServer.Manager.Host.Tests
{
    using Client.Configuration;
    using Client.ResourceOwners;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleAuth.Errors;
    using SimpleAuth.Helpers;
    using SimpleAuth.Shared.Requests;
    using Xunit;

    public class ResourceOwnerFixture : IClassFixture<TestManagerServerFixture>
    {
        private readonly TestManagerServerFixture _server;
        private IResourceOwnerClient _resourceOwnerClient;

        public ResourceOwnerFixture(TestManagerServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Trying_To_Get_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var resourceOwnerId = "invalid_login";
            var result = await _resourceOwnerClient.ResolveGet(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    resourceOwnerId,
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, resourceOwnerId), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_No_Login_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveAdd(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new AddResourceOwnerRequest(),
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("The parameter login is missing\r\nParameter name: Id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_No_Password_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveAdd(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new AddResourceOwnerRequest
                    {
                        Subject = "subject"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("The parameter password is missing\r\nParameter name: Password", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Login_Already_Exists_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveAdd(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new AddResourceOwnerRequest
                    {
                        Subject = "administrator",
                        Password = "password"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("a resource owner with same credentials already exists", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_And_No_Login_Is_Passwed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveUpdateClaims(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerClaimsRequest(),
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("The parameter login is missing\r\nParameter name: id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_And_Resource_Owner_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveUpdateClaims(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerClaimsRequest
                    {
                        Login = "invalid_login"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("The resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_No_Login_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveUpdatePassword(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerPasswordRequest(),
                    null)
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("The parameter login is missing\r\nParameter name: id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_No_Password_Is_Passed_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveUpdatePassword(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerPasswordRequest
                    {
                        Login = "login"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, "login"), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_Resource_Owner_Doesnt_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveUpdatePassword(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerPasswordRequest
                    {
                        Login = "invalid_login",
                        Password = "password"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, "invalid_login"), result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Delete_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveDelete(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    "invalid_login",
                    null)
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(ErrorDescriptions.TheResourceOwnerCannotBeRemoved, result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_Then_ResourceOwner_Is_Updated()
        {
            InitializeFakeObjects();


            var result = await _resourceOwnerClient.ResolveUpdateClaims(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerClaimsRequest
                    {
                        Login = "administrator",
                        Claims =
                            new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("role", "role"),
                                new KeyValuePair<string, string>("not_valid", "not_valid")
                            }
                    },
                    null)
                .ConfigureAwait(false);
            var resourceOwner = await _resourceOwnerClient.ResolveGet(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    "administrator",
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(resourceOwner);
            Assert.False(resourceOwner.ContainsError);
            Assert.Equal("role", resourceOwner.Content.Claims.First(c => c.Key == "role").Value);
        }

        [Fact]
        public async Task When_Update_Password_Then_ResourceOwner_Is_Updated()
        {
            InitializeFakeObjects();
            
            var result = await _resourceOwnerClient.ResolveUpdatePassword(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new UpdateResourceOwnerPasswordRequest
                    {
                        Login = "administrator",
                        Password = "pass"
                    },
                    null)
                .ConfigureAwait(false);
            var resourceOwner = await _resourceOwnerClient.ResolveGet(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    "administrator",
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(resourceOwner);
            Assert.Equal("pass".ToSha256Hash(), resourceOwner.Content.Password);
        }

        [Fact]
        public async Task When_Search_Resource_Owners_Then_One_Resource_Owner_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveSearch(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new SearchResourceOwnersRequest
                    {
                        StartIndex = 0,
                        NbResults = 1
                    },
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.Single(result.Content.Content);
            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Get_All_ResourceOwners_Then_All_Resource_Owners_Are_Returned()
        {
            InitializeFakeObjects();
            
            var resourceOwners =
                await _resourceOwnerClient.ResolveGetAll(
                        new Uri("http://localhost:5000/.well-known/openid-configuration"),
                        null)
                    .ConfigureAwait(false);

            Assert.NotNull(resourceOwners);
            Assert.False(resourceOwners.ContainsError);
            Assert.True(resourceOwners.Content.Any());
        }

        [Fact]
        public async Task When_Add_Resource_Owner_Then_Ok_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerClient.ResolveAdd(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new AddResourceOwnerRequest
                    {
                        Subject = "login",
                        Password = "password"
                    },
                    null)
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Delete_ResourceOwner_Then_ResourceOwner_Doesnt_Exist()
        {
            InitializeFakeObjects();


            var result = await _resourceOwnerClient.ResolveAdd(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    new AddResourceOwnerRequest
                    {
                        Subject = "login1",
                        Password = "password"
                    },
                    null)
                .ConfigureAwait(false);
            var remove = await _resourceOwnerClient.ResolveDelete(
                    new Uri("http://localhost:5000/.well-known/openid-configuration"),
                    "login1",
                    null)
                .ConfigureAwait(false);

            Assert.NotNull(remove);
            Assert.False(remove.ContainsError);
        }

        private void InitializeFakeObjects()
        {
            var addResourceOwnerOperation = new AddResourceOwnerOperation(_server.Client);
            var deleteResourceOwnerOperation = new DeleteResourceOwnerOperation(_server.Client);
            var getAllResourceOwnersOperation = new GetAllResourceOwnersOperation(_server.Client);
            var getResourceOwnerOperation = new GetResourceOwnerOperation(_server.Client);
            var updateResourceOwnerClaimsOperation = new UpdateResourceOwnerClaimsOperation(_server.Client);
            var updateResourceOwnerPasswordOperation = new UpdateResourceOwnerPasswordOperation(_server.Client);
            var getConfigurationOperation = new GetConfigurationOperation(_server.Client);
            var searchResourceOwnersOperation = new SearchResourceOwnersOperation(_server.Client);
            _resourceOwnerClient = new ResourceOwnerClient(
                addResourceOwnerOperation,
                deleteResourceOwnerOperation,
                getAllResourceOwnersOperation,
                getResourceOwnerOperation,
                updateResourceOwnerClaimsOperation,
                updateResourceOwnerPasswordOperation,
                getConfigurationOperation,
                searchResourceOwnersOperation);
        }
    }
}
