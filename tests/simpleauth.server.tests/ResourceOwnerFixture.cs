﻿namespace SimpleAuth.Server.Tests;

using Shared.Requests;
using SimpleAuth.Shared.Errors;
using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuth.Client;
using SimpleAuth.Properties;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
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
        var result =
            await _resourceOwnerClient.GetResourceOwner(resourceOwnerId, "token").ConfigureAwait(false) as
                Option<ResourceOwner>.Error;

        Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, result.Details.Detail);
    }

    [Fact]
    public async Task When_Pass_No_Password_To_Add_ResourceOwner_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient
            .AddResourceOwner(new AddResourceOwnerRequest {Subject = "subject"}, "token")
            .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
    }

    [Fact]
    public async Task When_Login_Already_Exists_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient.AddResourceOwner(
                new AddResourceOwnerRequest {Subject = "administrator", Password = "password"},
                "token")
            .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
        Assert.Equal("A resource owner with same credentials already exists", result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Claims_And_No_Login_Is_Passed_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient
            .UpdateResourceOwnerClaims(new UpdateResourceOwnerClaimsRequest(), "token")
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "login"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Claims_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient.UpdateResourceOwnerClaims(
                new UpdateResourceOwnerClaimsRequest {Subject = "invalid_login"},
                "token")
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal("The resource owner doesn't exist", result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_No_Login_Is_Passed_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient
            .UpdateResourceOwnerPassword(new UpdateResourceOwnerPasswordRequest(), "token")
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "login"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_No_Password_Is_Passed_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest {Subject = "login"},
                "token")
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "password"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Password_And_Resource_Owner_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var result = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest {Subject = "invalid_login", Password = "password"},
                "token")
            .ConfigureAwait(false) as Option.Error;

        Assert.Equal(ErrorCodes.InvalidParameterCode, result.Details.Title);
        Assert.Equal(Strings.TheRoDoesntExist, result.Details.Detail);
    }

    [Fact]
    public async Task When_Delete_Unknown_Resource_Owner_Then_Error_Is_Returned()
    {
        var result =
            await _resourceOwnerClient.DeleteResourceOwner("invalid_login", "token").ConfigureAwait(false) as
                Option.Error;

        Assert.Equal(ErrorCodes.UnhandledExceptionCode, result.Details.Title);
        Assert.Equal(Strings.TheResourceOwnerCannotBeRemoved, result.Details.Detail);
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
                        new ClaimData {Type = "role", Value = "role"},
                        new ClaimData {Type = "not_valid", Value = "not_valid"}
                    }
                },
                "token")
            .ConfigureAwait(false);
        var resourceOwner =
            await _resourceOwnerClient.GetResourceOwner("administrator", "token").ConfigureAwait(false) as
                Option<ResourceOwner>.Result;

        Assert.Equal("role", resourceOwner.Item.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public async Task When_Update_Password_Then_ResourceOwner_Is_Updated()
    {
        _ = await _resourceOwnerClient.UpdateResourceOwnerPassword(
                new UpdateResourceOwnerPasswordRequest {Subject = "administrator", Password = "pass"},
                "token")
            .ConfigureAwait(false);
        var resourceOwner =
            await _resourceOwnerClient.GetResourceOwner("administrator", "token").ConfigureAwait(false) as
                Option<ResourceOwner>.Result;

        Assert.Equal("pass".ToSha256Hash(string.Empty), resourceOwner.Item.Password);
    }

    [Fact]
    public async Task When_Search_Resource_Owners_Then_One_Resource_Owner_Is_Returned()
    {
        var result = await _resourceOwnerClient.SearchResourceOwners(
                new SearchResourceOwnersRequest {StartIndex = 0, NbResults = 1},
                "token")
            .ConfigureAwait(false) as Option<PagedResult<ResourceOwner>>.Result;

        Assert.Single(result.Item.Content);
    }

    [Fact]
    public async Task When_Get_All_ResourceOwners_Then_All_Resource_Owners_Are_Returned()
    {
        var resourceOwners = await _resourceOwnerClient.GetAllResourceOwners("token") // "administrator"
            .ConfigureAwait(false) as Option<ResourceOwner[]>.Result;

        Assert.NotEmpty(resourceOwners.Item);
    }

    [Fact]
    public async Task When_Add_Resource_Owner_Then_Ok_Is_Returned()
    {
        var result = await _resourceOwnerClient.AddResourceOwner(
                new AddResourceOwnerRequest {Subject = "login", Password = "password"},
                "token")
            .ConfigureAwait(false);

        Assert.IsType<Option<AddResourceOwnerResponse>.Result>(result);
    }

    [Fact]
    public async Task When_Delete_ResourceOwner_Then_ResourceOwner_Does_Not_Exist()
    {
        var result = await _resourceOwnerClient.AddResourceOwner(
                new AddResourceOwnerRequest {Subject = "login1", Password = "password"},
                "token")
            .ConfigureAwait(false) as Option<AddResourceOwnerResponse>.Result;
        var remove = await _resourceOwnerClient.DeleteResourceOwner(result.Item.Subject, "token")
            .ConfigureAwait(false);

        Assert.IsType<Option.Success>(remove);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server?.Dispose();
    }
}