﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Server.Tests.Apis;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth;
using DotAuth.Api.PermissionController;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class AddPermissionActionFixture
{
    private readonly ITestOutputHelper _outputHelper;
    private IResourceSetRepository _resourceSetRepositoryStub = null!;
    private ITicketStore _ticketStoreStub = null!;
    private RuntimeSettings _configurationServiceStub = null!;
    private RequestPermissionHandler _requestPermissionHandler = null!;

    public AddPermissionActionFixture(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task When_RequiredParameter_ResourceSetId_Is_Not_Specified_Then_Exception_Is_Thrown()
    {
        InitializeFakeObjects(new ResourceSet { Id = DotAuth.Id.Create(), Name = "resource" });
        var addPermissionParameter = new PermissionRequest();

        var exception = Assert.IsType<Option<Ticket>.Error>(await _requestPermissionHandler
            .Execute("tester", CancellationToken.None, addPermissionParameter));
        Assert.Equal(ErrorCodes.InvalidRequest, exception.Details.Title);
        Assert.Equal(
            string.Format(
                Strings.MissingParameter,
                UmaConstants.AddPermissionNames.ResourceSetId),
            exception.Details.Detail);
    }

    [Fact]
    public async Task When_RequiredParameter_Scopes_Is_Not_Specified_Then_Exception_Is_Thrown()
    {
        InitializeFakeObjects(new ResourceSet { Id = DotAuth.Id.Create(), Name = "resource" });
        var addPermissionParameter = new PermissionRequest { ResourceSetId = "resource_set_id" };

        var exception = Assert.IsType<Option<Ticket>.Error>(
            await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter));
        Assert.Equal(ErrorCodes.InvalidRequest, exception.Details.Title);
        Assert.Equal(
            string.Format(Strings.MissingParameter, UmaConstants.AddPermissionNames.Scopes),
            exception.Details.Detail);
    }

    [Fact]
    public async Task When_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        const string resourceSetId = "resource_set_id";
        InitializeFakeObjects(new ResourceSet { Id = DotAuth.Id.Create(), Name = "resource" });
        var addPermissionParameter =
            new PermissionRequest { ResourceSetId = resourceSetId, Scopes = new[] { "scope" } };

        var exception = Assert.IsType<Option<Ticket>.Error>(
            await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter));
        Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Details.Title);
        Assert.Equal(string.Format(Strings.TheResourceSetDoesntExist, resourceSetId), exception.Details.Detail);
    }

    [Fact]
    public async Task When_Scope_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        const string resourceSetId = "resource_set_id";
        var addPermissionParameter = new PermissionRequest
        {
            ResourceSetId = resourceSetId,
            Scopes = new[] { ErrorCodes.InvalidScope }
        };
        var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
        InitializeFakeObjects(resources);

        var exception = Assert.IsType<Option<Ticket>.Error>(
            await _requestPermissionHandler.Execute("tester", CancellationToken.None, addPermissionParameter));
        Assert.Equal(ErrorCodes.InvalidScope, exception.Details.Title);
        Assert.Equal(Strings.TheScopeAreNotValid, exception.Details.Detail);
    }

    [Fact]
    public async Task When_Adding_Permission_Then_TicketId_Is_Returned()
    {
        var handler = new JwtSecurityTokenHandler();
        var idToken = handler.CreateEncodedJwt(
            "test",
            "test",
            new ClaimsIdentity(new[] { new Claim("sub", "tester") }),
            null,
            null,
            null,
            null);
        const string resourceSetId = "resource_set_id";
        var addPermissionParameter = new PermissionRequest
        {
            ResourceSetId = resourceSetId,
            Scopes = new[] { "scope" },
            IdToken = idToken
        };
        var resources = new[] { new ResourceSet { Id = resourceSetId, Scopes = new[] { "scope" } } };
        InitializeFakeObjects(resources);
        _ticketStoreStub.Add(Arg.Any<Ticket>(), Arg.Any<CancellationToken>()).Returns(true);

        var ticket = Assert.IsType<Option<Ticket>.Result>( await _requestPermissionHandler
            .Execute("tester", CancellationToken.None, addPermissionParameter));

        Assert.NotEmpty(ticket.Item.Requester);
    }

    private void InitializeFakeObjects(params ResourceSet[] resourceSets)
    {
        _resourceSetRepositoryStub = Substitute.For<IResourceSetRepository>();
        _resourceSetRepositoryStub.Get(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                c => Task.FromResult(resourceSets.FirstOrDefault(x => x.Id == c[1].ToString())));
        _resourceSetRepositoryStub.Get(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(resourceSets);
        _ticketStoreStub = Substitute.For<ITicketStore>();
        _configurationServiceStub = new RuntimeSettings(ticketLifeTime: TimeSpan.FromSeconds(2));
        _requestPermissionHandler = new RequestPermissionHandler(
            new InMemoryTokenStore(),
            _resourceSetRepositoryStub,
            _configurationServiceStub,
            new TestOutputLogger("test", _outputHelper));
    }
}
