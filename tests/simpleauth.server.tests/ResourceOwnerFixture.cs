namespace SimpleAuth.Server.Tests
{
    using Shared.Requests;
    using SimpleAuth.Extensions;
    using SimpleAuth.Manager.Client;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using Xunit;

    public class ResourceOwnerFixture
    {
        private const string LocalhostWellKnownOpenidConfiguration =
            "http://localhost:5000/.well-known/openid-configuration";

        private readonly TestManagerServerFixture _server;
        private readonly ManagementClient _resourceOwnerClient;

        public ResourceOwnerFixture()
        {
            _server = new TestManagerServerFixture();
            _resourceOwnerClient = ManagementClient.Create(
                    _server.Client,
                    new Uri(LocalhostWellKnownOpenidConfiguration))
                .Result;
        }

        [Fact]
        public async Task When_Trying_To_Get_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            var resourceOwnerId = "invalid_login";
            var result = await _resourceOwnerClient.GetResourceOwner(
                    resourceOwnerId)
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, result.Error.Error);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, resourceOwnerId),
                result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_No_Login_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.AddResourceOwner(
                    new AddResourceOwnerRequest())
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal($"The parameter login is missing{Environment.NewLine}Parameter name: id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Pass_No_Password_To_Add_ResourceOwner_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.AddResourceOwner(
                    new AddResourceOwnerRequest { Subject = "subject" })
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
        }

        [Fact]
        public async Task When_Login_Already_Exists_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.AddResourceOwner(
                    new AddResourceOwnerRequest { Subject = "administrator", Password = "password" })
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal("a resource owner with same credentials already exists", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_And_No_Login_Is_Passwed_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerClaims(
                    new UpdateResourceOwnerClaimsRequest())
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal($"The parameter login is missing{Environment.NewLine}Parameter name: id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerClaims(
                    new UpdateResourceOwnerClaimsRequest { Subject = "invalid_login" })
                .ConfigureAwait(false);

            Assert.True(result.ContainsError);
            Assert.Equal(ErrorCodes.InvalidParameterCode, result.Error.Error);
            Assert.Equal("The resource owner invalid_login doesn't exist", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_No_Login_Is_Passed_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                    new UpdateResourceOwnerPasswordRequest())
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal($"The parameter login is missing{Environment.NewLine}Parameter name: id", result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_No_Password_Is_Passed_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                    new UpdateResourceOwnerPasswordRequest { Subject = "login" })
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidParameterCode, result.Error.Error);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, "login"),
                result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Password_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                    new UpdateResourceOwnerPasswordRequest {Subject = "invalid_login", Password = "password"})
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidParameterCode, result.Error.Error);
            Assert.Equal(
                string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, "invalid_login"),
                result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Delete_Unknown_Resource_Owner_Then_Error_Is_Returned()
        {
            var result = await _resourceOwnerClient.DeleteResourceOwner(
                    "invalid_login")
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Error.Error);
            Assert.Equal(ErrorDescriptions.TheResourceOwnerCannotBeRemoved, result.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Update_Claims_Then_ResourceOwner_Is_Updated()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerClaims(
                    new UpdateResourceOwnerClaimsRequest
                    {
                        Subject = "administrator",
                        Claims = new[]
                        {
                            new PostClaim {Type = "role", Value = "role"},
                            new PostClaim {Type = "not_valid", Value = "not_valid"}
                        }
                    })
                .ConfigureAwait(false);
            var resourceOwner = await _resourceOwnerClient.GetResourceOwner(
                    "administrator")
                .ConfigureAwait(false);

            Assert.False(resourceOwner.ContainsError);
            Assert.Equal("role", resourceOwner.Content.Claims.First(c => c.Type == "role").Value);
        }

        [Fact]
        public async Task When_Update_Password_Then_ResourceOwner_Is_Updated()
        {
            var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                    new UpdateResourceOwnerPasswordRequest { Subject = "administrator", Password = "pass" })
                .ConfigureAwait(false);
            var resourceOwner = await _resourceOwnerClient.GetResourceOwner(
                    "administrator")
                .ConfigureAwait(false);

            Assert.Equal("pass".ToSha256Hash(), resourceOwner.Content.Password);
        }

        [Fact]
        public async Task When_Search_Resource_Owners_Then_One_Resource_Owner_Is_Returned()
        {
            var result = await _resourceOwnerClient.SearchResourceOwners(
                    new SearchResourceOwnersRequest { StartIndex = 0, NbResults = 1 })
                .ConfigureAwait(false);

            Assert.Single(result.Content.Content);
            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Get_All_ResourceOwners_Then_All_Resource_Owners_Are_Returned()
        {
            var resourceOwners = await _resourceOwnerClient
                .GetAllResourceOwners() // "administrator"
                .ConfigureAwait(false);

            Assert.False(resourceOwners.ContainsError);
            Assert.NotEmpty(resourceOwners.Content);
        }

        [Fact]
        public async Task When_Add_Resource_Owner_Then_Ok_Is_Returned()
        {
            var result = await _resourceOwnerClient.AddResourceOwner(
                    new AddResourceOwnerRequest { Subject = "login", Password = "password" })
                .ConfigureAwait(false);

            Assert.False(result.ContainsError);
        }

        [Fact]
        public async Task When_Delete_ResourceOwner_Then_ResourceOwner_Does_Not_Exist()
        {
            var result = await _resourceOwnerClient.AddResourceOwner(
                    new AddResourceOwnerRequest { Subject = "login1", Password = "password" })
                .ConfigureAwait(false);
            var remove = await _resourceOwnerClient.DeleteResourceOwner(
                    "login1")
                .ConfigureAwait(false);

            Assert.False(remove.ContainsError);
        }
    }
}
