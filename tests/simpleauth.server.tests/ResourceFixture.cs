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

namespace SimpleAuth.Server.Tests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SimpleAuth.Client;
using SimpleAuth.Properties;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Requests;
using SimpleAuth.Shared.Responses;
using Xunit;
using Xunit.Abstractions;

public sealed class ResourceFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";
    private readonly UmaClient _umaClient;
    private readonly TestUmaServer _server;

    public ResourceFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestUmaServer(outputHelper);
        _umaClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration));
    }

    [Fact]
    public async Task When_Add_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
    {
        var resource = await _umaClient.AddResource(new ResourceSet { Name = string.Empty }, "header")
            .ConfigureAwait(false) as Option<AddResourceSetResponse>.Error;

        Assert.NotNull(resource);
        Assert.Equal(ErrorCodes.InvalidRequest, resource.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "name"), resource.Details.Detail);
    }

    [Fact]
    public async Task When_Add_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
    {
        var resource =
            await _umaClient.AddResource(new ResourceSet { Name = "name" }, "header").ConfigureAwait(false) as
                Option<AddResourceSetResponse>.Error;

        Assert.Equal(ErrorCodes.InvalidRequest, resource.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "scopes"), resource.Details.Detail);
    }

    [Fact]
    public async Task When_Add_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
    {
        var request = new { name = "name", scopes = new[] { "scope" }, icon_uri = "invalid" };
        var serializedPostResourceSet = Serializer.Default.Serialize(request);
        var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage
        {
            Content = body,
            Method = HttpMethod.Post,
            RequestUri = new Uri(BaseUrl + "/rs/resource_set")
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "header");
        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);

        Assert.False(httpResult.IsSuccessStatusCode);
    }

    [Fact]
    public async Task When_Get_Unknown_Resource_Then_Error_Is_Returned()
    {
        var resource =
            await _umaClient.GetResource("unknown", "header").ConfigureAwait(false) as Option<ResourceSet>.Error;

        Assert.Equal(HttpStatusCode.NoContent, resource.Details.Status);
    }

    [Fact]
    public async Task When_Delete_Unknown_Resource_Then_Error_Is_Returned()
    {
        var resource = await _umaClient.DeleteResource("unknown", "header").ConfigureAwait(false) as Option.Error;

        Assert.Equal(HttpStatusCode.BadRequest, resource.Details.Status);
    }

    [Fact]
    public async Task WhenUpdateResourceAndNoIdIsSpecifiedThenIsNotUpdated()
    {
        var resource =
            await _umaClient.UpdateResource(new ResourceSet(), "header").ConfigureAwait(false) as
                Option<UpdateResourceSetResponse>.Error;

        Assert.Equal(HttpStatusCode.NotFound, resource.Details.Status);
    }

    [Fact]
    public async Task When_Update_Resource_And_No_Name_Is_Specified_Then_Error_Is_Returned()
    {
        var resource = await _umaClient.UpdateResource(
                new ResourceSet { Id = "invalid", Name = string.Empty },
                "header")
            .ConfigureAwait(false) as Option<UpdateResourceSetResponse>.Error;

        Assert.Equal(ErrorCodes.InvalidRequest, resource.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "name"), resource.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Resource_And_No_Scopes_Is_Specified_Then_Error_Is_Returned()
    {
        var resource = await _umaClient.UpdateResource(new ResourceSet { Id = "invalid", Name = "name" }, "header")
            .ConfigureAwait(false) as Option<UpdateResourceSetResponse>.Error;

        Assert.Equal(ErrorCodes.InvalidRequest, resource.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "scopes"), resource.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Resource_And_No_Invalid_IconUri_Is_Specified_Then_Error_Is_Returned()
    {
        var request = new { _id = "invalid", name = "name", scopes = new[] { "scope" }, icon_uri = "invalid" };
        var serializedPostResourceSet = Serializer.Default.Serialize(request);
        var body = new StringContent(serializedPostResourceSet, Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage
        {
            Content = body,
            Method = HttpMethod.Put,
            RequestUri = new Uri(BaseUrl + "/rs/resource_set")
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "header");
        var httpResult = await _server.Client().SendAsync(httpRequest).ConfigureAwait(false);

        Assert.False(httpResult.IsSuccessStatusCode);
    }

    [Fact]
    public async Task When_Update_Unknown_Resource_Then_Error_Is_Returned()
    {
        var resource = await _umaClient.UpdateResource(
                new ResourceSet { Id = "invalid", Name = "name", Scopes = new[] { "scope" } },
                "header")
            .ConfigureAwait(false) as Option<UpdateResourceSetResponse>.Error;

        Assert.Equal("not_updated", resource.Details.Title);
        Assert.Equal("Resource cannot be updated", resource.Details.Detail);
    }

    [Fact]
    public async Task When_Getting_Resources_Then_Identifiers_Are_Returned()
    {
        var resources = await _umaClient.GetAllResources("token").ConfigureAwait(false) as Option<string[]>.Result;

        Assert.True(resources.Item.Any());
    }

    [Fact]
    public async Task When_Getting_ResourceInformation_Then_Dto_Is_Returned()
    {
        var resources = await _umaClient.GetAllResources("header").ConfigureAwait(false) as Option<string[]>.Result;
        var resource = await _umaClient.GetResource(resources.Item.First(), "header").ConfigureAwait(false);

        Assert.NotNull(resource);
    }

    [Fact]
    public async Task When_Deleting_ResourceInformation_Then_It_Does_Not_Exist()
    {
        var resources = await _umaClient.GetAllResources("header").ConfigureAwait(false) as Option<string[]>.Result;
        await _umaClient.DeleteResource(resources.Item.First(), "header").ConfigureAwait(false);
        var information = await _umaClient.GetResource(resources.Item.First(), "header").ConfigureAwait(false);

        Assert.IsType<Option<ResourceSet>.Error>(information);
    }

    [Fact]
    public async Task When_Adding_Resource_Then_Information_Can_Be_Retrieved()
    {
        var resource = await _umaClient.AddResource(
                new ResourceSet { Name = "name", Scopes = new[] { "scope" } },
                "header")
            .ConfigureAwait(false);

        Assert.NotNull(resource);
    }

    [Fact]
    public async Task When_Search_Resources_Then_List_Is_Returned()
    {
        var resource = await _umaClient.SearchResources(
                new SearchResourceSet { StartIndex = 0, TotalResults = 100 },
                "header")
            .ConfigureAwait(false) as Option<PagedResult<ResourceSet>>.Result;

        Assert.True(resource.Item.Content.Any());
    }

    [Fact]
    public async Task When_Updating_Resource_Then_Changes_Are_Persisted()
    {
        var resource = await _umaClient.AddResource(
                new ResourceSet { Name = "name", Scopes = new[] { "scope" } },
                "header")
            .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;

        var updateResult = await _umaClient.UpdateResource(
                new ResourceSet { Id = resource.Item.Id, Name = "name2", Type = "type", Scopes = new[] { "scope2" } },
                "header")
            .ConfigureAwait(false) as Option<UpdateResourceSetResponse>.Result;
        var information =
            await _umaClient.GetResource(updateResult.Item.Id, "header").ConfigureAwait(false) as
                Option<ResourceSet>.Result;

        Assert.Equal("name2", information.Item.Name);
        Assert.Equal("type", information.Item.Type);
        Assert.Equal("scope2", information.Item.Scopes.Single());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server?.Dispose();
    }
}