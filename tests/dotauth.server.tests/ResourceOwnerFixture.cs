namespace DotAuth.Server.Tests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Xunit;

public sealed class ResourceOwnerFixture : IDisposable
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
        var result = Assert.IsType<Option<ResourceOwner>.Error>(
            await _resourceOwnerClient.GetResourceOwner(resourceOwnerId, "token",
                TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, result.Details.Detail);
    }

    [Fact]
    public async Task When_Getting_Resource_Owners_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.ResourceOwners}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Manage Users", content);
    }

    [Fact]
    public async Task When_Getting_Resource_Owner_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.ResourceOwners}/administrator");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("administrator", content);
        Assert.Contains("Claims", content);
    }

    [Fact]
    public async Task When_Pass_No_Password_To_Add_ResourceOwner_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option<AddResourceOwnerResponse>.Error>(
            await _resourceOwnerClient
                .AddResourceOwner(new AddResourceOwnerRequest { Subject = "subject" }, "token",
                    TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
    }

    [Fact]
    public async Task When_Login_Already_Exists_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option<AddResourceOwnerResponse>.Error>(
            await _resourceOwnerClient.AddResourceOwner(
                new AddResourceOwnerRequest { Subject = "administrator", Password = "password" },
                "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
        Assert.Equal("A resource owner with same credentials already exists", result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Claims_And_No_Login_Is_Passed_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option.Error>(await _resourceOwnerClient
            .UpdateResourceOwnerClaims(new UpdateResourceOwnerClaimsRequest(), "token",
                TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "login"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Claims_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option.Error>(await _resourceOwnerClient.UpdateResourceOwnerClaims(
            new UpdateResourceOwnerClaimsRequest { Subject = "invalid_login" },
            "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal("The resource owner doesn't exist", result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_No_Login_Is_Passed_Then_Error_Is_Returned()
    {
        var option = await _resourceOwnerClient
                .UpdateResourceOwnerPassword(new UpdateResourceOwnerPasswordRequest(), "token",
                    TestContext.Current.CancellationToken)
            as Option.Error;

        var result = Assert.IsType<Option.Error>(option);
        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "login"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_No_Password_Is_Passed_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option.Error>(await _resourceOwnerClient.UpdateResourceOwnerPassword(
            new UpdateResourceOwnerPasswordRequest { Subject = "login" },
            "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "password"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option.Error>(await _resourceOwnerClient.UpdateResourceOwnerPassword(
            new UpdateResourceOwnerPasswordRequest { Subject = "invalid_login", Password = "password" },
            "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, result.Details.Detail);
    }

    [Fact]
    public async Task When_Delete_Unknown_Resource_Owner_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option.Error>(
            await _resourceOwnerClient.DeleteResourceOwner("invalid_login", "token",
                TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
        Assert.Equal(Strings.TheResourceOwnerCannotBeRemoved, result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Claims_Then_ResourceOwner_Is_Updated()
    {
        _ = await _resourceOwnerClient.UpdateResourceOwnerClaims(
                new UpdateResourceOwnerClaimsRequest
                {
                    Subject = "administrator",
                    Claims =
                    [
                        new ClaimData { Type = "role", Value = "role" },
                        new ClaimData { Type = "not_valid", Value = "not_valid" }
                    ]
                },
                "token", TestContext.Current.CancellationToken)
            ;
        var resourceOwner = Assert.IsType<Option<ResourceOwner>.Result>(
            await _resourceOwnerClient.GetResourceOwner("administrator", "token",
                TestContext.Current.CancellationToken));

        Assert.Equal("role", resourceOwner.Item.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public async Task When_Update_Password_Then_ResourceOwner_Is_Updated()
    {
        _ = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest { Subject = "administrator", Password = "pass" },
                "token", TestContext.Current.CancellationToken)
            ;
        var resourceOwner = Assert.IsType<Option<ResourceOwner>.Result>(
            await _resourceOwnerClient.GetResourceOwner("administrator", "token",
                TestContext.Current.CancellationToken));

        Assert.Equal("pass".ToSha256Hash(string.Empty), resourceOwner.Item.Password);
    }

    [Fact]
    public async Task When_Search_Resource_Owners_Then_One_Resource_Owner_Is_Returned()
    {
        var result = Assert.IsType<Option<PagedResult<ResourceOwner>>.Result>(
            await _resourceOwnerClient.SearchResourceOwners(
                new SearchResourceOwnersRequest { StartIndex = 0, NbResults = 1 },
                "token", TestContext.Current.CancellationToken)
        );

        Assert.Single(result.Item.Content);
    }

    [Fact]
    public async Task When_Get_All_ResourceOwners_Then_All_Resource_Owners_Are_Returned()
    {
        var resourceOwners = Assert.IsType<Option<ResourceOwner[]>.Result>(
            await _resourceOwnerClient.GetAllResourceOwners("token",
                TestContext.Current.CancellationToken) // "administrator"
        );

        Assert.NotEmpty(resourceOwners.Item);
    }

    [Fact]
    public async Task When_Add_Resource_Owner_Then_Ok_Is_Returned()
    {
        var result = await _resourceOwnerClient.AddResourceOwner(
            new AddResourceOwnerRequest { Subject = "login", Password = "password" },
            "token", TestContext.Current.CancellationToken);

        Assert.IsType<Option<AddResourceOwnerResponse>.Result>(result);
    }

    [Fact]
    public async Task When_Delete_ResourceOwner_Then_ResourceOwner_Does_Not_Exist()
    {
        var result = Assert.IsType<Option<AddResourceOwnerResponse>.Result>(await _resourceOwnerClient.AddResourceOwner(
            new AddResourceOwnerRequest { Subject = "login1", Password = "password" },
            "token", TestContext.Current.CancellationToken)
        );
        var remove = await _resourceOwnerClient.DeleteResourceOwner(result.Item.Subject!, "token",
            TestContext.Current.CancellationToken);

        Assert.IsType<Option.Success>(remove);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server.Dispose();
    }
}
