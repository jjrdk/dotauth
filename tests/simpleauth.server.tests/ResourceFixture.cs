// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Uma.Client;
    using Xunit;

    public class ResourceFixture
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";
        private readonly UmaClient _umaClient;
        private readonly TestUmaServerFixture _server;

        public ResourceFixture()
        {
            _server = new TestUmaServerFixture();
            _umaClient = UmaClient.Create(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration)).Result;
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.AddResource(new PostResourceSet {Name = string.Empty}, "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the parameter name needs to be specified", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.AddResource(new PostResourceSet {Name = "name"}, "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Add_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "name", Scopes = new[] {"scope"}, IconUri = "invalid"},
                    "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the url invalid is not well formed", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Get_Unknown_Resource_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.GetResource("unknown", "header").ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Title);
            Assert.Equal("resource cannot be found", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Delete_Unknown_Resource_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.DeleteResource("unknown", "header").ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(HttpStatusCode.BadRequest, resource.Error.Status);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Id_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.UpdateResource(new PutResourceSet(), "header").ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the parameter id needs to be specified", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.UpdateResource(
                    new PutResourceSet {Id = "invalid", Name = string.Empty},
                    "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the parameter name needs to be specified", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.UpdateResource(new PutResourceSet {Id = "invalid", Name = "name"}, "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the parameter scopes needs to be specified", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.UpdateResource(
                    new PutResourceSet {Id = "invalid", Name = "name", Scopes = new[] {"scope"}, IconUri = "invalid"},
                    "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal(ErrorCodes.InvalidRequestCode, resource.Error.Title);
            Assert.Equal("the url invalid is not well formed", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Update_Unknown_Resource_Then_Error_Is_Returned()
        {
            var resource = await _umaClient.UpdateResource(
                    new PutResourceSet {Id = "invalid", Name = "name", Scopes = new[] {"scope"}},
                    "header")
                .ConfigureAwait(false);

            Assert.True(resource.ContainsError);
            Assert.Equal("not_found", resource.Error.Title);
            Assert.Equal("resource cannot be found", resource.Error.Detail);
        }

        [Fact]
        public async Task When_Getting_Resources_Then_Identifiers_Are_Returned()
        {
            var resources = await _umaClient.GetAllResources("token").ConfigureAwait(false);

            Assert.True(resources.Content.Any());
        }

        [Fact]
        public async Task When_Getting_ResourceInformation_Then_Dto_Is_Returned()
        {
            var resources = await _umaClient.GetAllResources("header").ConfigureAwait(false);
            var resource = await _umaClient.GetResource(resources.Content.First(), "header").ConfigureAwait(false);

            Assert.NotNull(resource);
        }

        [Fact]
        public async Task When_Deleting_ResourceInformation_Then_It_Does_Not_Exist()
        {
            var resources = await _umaClient.GetAllResources("header").ConfigureAwait(false);
            var resource = await _umaClient.DeleteResource(resources.Content.First(), "header").ConfigureAwait(false);
            var information = await _umaClient.GetResource(resources.Content.First(), "header").ConfigureAwait(false);

            Assert.False(resource.ContainsError);
            Assert.True(information.ContainsError);
        }

        [Fact]
        public async Task When_Adding_Resource_Then_Information_Can_Be_Retrieved()
        {
            var resource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "name", Scopes = new[] {"scope"}},
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(resource);
        }

        [Fact]
        public async Task When_Search_Resources_Then_List_Is_Returned()
        {
            var resource = await _umaClient.SearchResources(
                    new SearchResourceSet {StartIndex = 0, TotalResults = 100},
                    "header")
                .ConfigureAwait(false);

            Assert.False(resource.ContainsError);
            Assert.True(resource.Content.Content.Any());
        }

        [Fact]
        public async Task When_Updating_Resource_Then_Changes_Are_Persisted()
        {
            var resource = await _umaClient.AddResource(
                    new PostResourceSet {Name = "name", Scopes = new[] {"scope"}},
                    "header")
                .ConfigureAwait(false);

            var updateResult = await _umaClient.UpdateResource(
                    new PutResourceSet
                        {
                            Id = resource.Content.Id, Name = "name2", Type = "type", Scopes = new[] {"scope2"}
                        },
                    "header")
                .ConfigureAwait(false);
            var information = await _umaClient.GetResource(updateResult.Content.Id, "header").ConfigureAwait(false);

            Assert.Equal("name2", information.Content.Name);
            Assert.Equal("type", information.Content.Type);
            Assert.Equal("scope2", information.Content.Scopes.Single());
        }
    }
}
