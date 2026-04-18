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

namespace DotAuth.Server.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Xunit;

public sealed class PermissionFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";

    private readonly UmaClient _umaClient;
    private readonly TestUmaServer _server;

    public PermissionFixture(ITestOutputHelper outputHelper)
    {
        _server = new TestUmaServer(outputHelper);
        _umaClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration));
    }

    [Fact]
    public async Task When_ResourceSetId_Is_Null_Then_Error_Is_Returned()
    {
        var ticket = Assert.IsType<Option<TicketResponse>.Error>(await _umaClient.RequestPermission(
            "header",
            requests: new PermissionRequest { ResourceSetId = string.Empty },
            cancellationToken: TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidRequest, ticket.Details.Title);
        Assert.Equal("The parameter resource_set_id needs to be specified", ticket.Details.Detail);
    }

    [Fact]
    public async Task When_Scopes_Is_Null_Then_Error_Is_Returned()
    {
        var ticket = Assert.IsType<Option<TicketResponse>.Error>(
            await _umaClient.RequestPermission(
                "header",
                requests: new PermissionRequest { ResourceSetId = "resource" },
                cancellationToken: TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidRequest, ticket.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "scopes"), ticket.Details.Detail);
    }

    [Fact]
    public async Task When_Resource_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var ticket = Assert.IsType<Option<TicketResponse>.Error>(
            await _umaClient.RequestPermission(
                "header",
                requests: new PermissionRequest { ResourceSetId = "resource", Scopes = ["scope"] },
                cancellationToken: TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidResourceSetId, ticket.Details.Title);
        Assert.Equal(string.Format(Strings.TheResourceSetDoesntExist, "resource"), ticket.Details.Detail);
    }

    [Fact]
    public async Task When_Scopes_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var resource = Assert.IsType<Option<AddResourceSetResponse>.Result>(
            await _umaClient.AddResourceSet(
                new ResourceSet { Name = "picture", Scopes = ["read"] },
                "header",
                cancellationToken: TestContext.Current.CancellationToken)
        );

        var ticket = Assert.IsType<Option<TicketResponse>.Error>(
            await _umaClient.RequestPermission(
                "header",
                requests: new PermissionRequest
                {
                    ResourceSetId = resource.Item.Id, Scopes = ["scopescopescope"]
                },
                cancellationToken: TestContext.Current.CancellationToken)
        );

        Assert.Equal(ErrorCodes.InvalidScope, ticket.Details.Title);
        Assert.Equal("one or more scopes are not valid", ticket.Details.Detail);
    }

    [Fact]
    public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
    {
        var resource = Assert.IsType<Option<AddResourceSetResponse>.Result>(
            await _umaClient.AddResourceSet(
                new ResourceSet { Name = "picture", Scopes = ["read"] },
                "header",
                cancellationToken: TestContext.Current.CancellationToken)
        );

        var ticket = Assert.IsType<Option<TicketResponse>.Result>(await _umaClient.RequestPermission(
            "header",
            requests: new PermissionRequest { ResourceSetId = resource.Item.Id, Scopes = ["read"] },
            cancellationToken: TestContext.Current.CancellationToken)
        );

        Assert.NotEmpty(ticket.Item.TicketId);
    }

    [Fact]
    public async Task WhenRequestingPermissionsThenTicketIdsAreReturned()
    {
        var resource = Assert.IsType<Option<AddResourceSetResponse>.Result>(await _umaClient.AddResourceSet(
            new ResourceSet { Name = "picture", Scopes = ["read"] },
            "header",
            cancellationToken: TestContext.Current.CancellationToken)
        );
        var permissions = new[]
        {
            new PermissionRequest { ResourceSetId = resource.Item.Id, Scopes = ["read"] },
            new PermissionRequest { ResourceSetId = resource.Item.Id, Scopes = ["read"] }
        };

        var ticket = await _umaClient.RequestPermission("header", CancellationToken.None, permissions)
            ;

        Assert.NotNull(ticket);
    }

    [Fact]
    public async Task When_Getting_Permission_Page_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{UmaConstants.RouteValues.Permission}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "header");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("No Open Permissions", content);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _server?.Dispose();
    }
}
