// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Tests.Api.Clients.Actions;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public sealed class UpdateClientActionFixture
{
    private IClientRepository _clientRepositoryMock = null!;
    private IScopeRepository _scopeRepositoryStub = null!;

    [Fact]
    public async Task WhenNoClientIdIsPassedThenReturnsError()
    {
        InitializeFakeObjects();
        var parameter = new Client { ClientId = "" };

        var result = await _clientRepositoryMock.Update(parameter, CancellationToken.None);

        Assert.IsType<Option.Error>(result);
    }

    [Fact]
    public async Task WhenClientDoesNotExistThenReturnsError()
    {
        const string clientId = "invalid_client_id";
        InitializeFakeObjects();

        var parameter = new Client
        {
            ClientId = clientId,
            RedirectionUrls = [new Uri("https://localhost")],
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "test" }]
        };

        var result = await _clientRepositoryMock.Update(parameter, CancellationToken.None);

        Assert.IsType<Option.Error>(result);
    }

    [Fact]
    public async Task WhenScopeAreNotSupportedThenErrorIsReturned()
    {
        const string clientId = "client_id";
        var client = new Client { ClientId = clientId };
        var parameter = new Client
        {
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            ClientId = clientId,
            AllowedScopes = ["not_supported_scope"],
            RedirectionUrls = [new Uri("https://localhost")]
        };
        InitializeFakeObjects([client]);

        _scopeRepositoryStub.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns([new Scope { Name = "scope" }]);

        var ex = Assert.IsType<Option.Error>(
            await _clientRepositoryMock.Update(parameter, CancellationToken.None));

        Assert.Equal("Unknown scopes: not_supported_scope", ex.Details.Detail);
    }

    private void InitializeFakeObjects(IReadOnlyCollection<Client>? clients = null)
    {
        _clientRepositoryMock = new InMemoryClientRepository(
            Substitute.For<IHttpClientFactory>(),
            new InMemoryScopeRepository(),
            Substitute.For<ILogger<InMemoryClientRepository>>(),
            clients ?? Array.Empty<Client>());
        _scopeRepositoryStub = Substitute.For<IScopeRepository>();
    }
}
