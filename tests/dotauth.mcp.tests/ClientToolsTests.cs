// Copyright © 2018 Jacob Reimers
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

namespace DotAuth.Mcp.Tests;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Mcp.Tools;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class ClientToolsTests
{
    private readonly IClientStore _store;
    private readonly ClientTools _sut;

    public ClientToolsTests()
    {
        _store = Substitute.For<IClientStore>();
        _sut = new ClientTools(_store);
    }

    [Fact]
    public async Task ListClients_returns_all_clients_without_secrets()
    {
        _store.GetAll(Arg.Any<CancellationToken>())
            .Returns(
            [
                new Client { ClientId = "c1", ClientName = "Client One" },
                new Client { ClientId = "c2", ClientName = "Client Two" }
            ]);

        var result = await _sut.ListClients(CancellationToken.None);

        Assert.Contains("c1", result);
        Assert.Contains("c2", result);
        // The Secrets array must not appear in the serialised output.
        Assert.DoesNotContain("\"secrets\"", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetClient_returns_not_found_for_unknown_id()
    {
        _store.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        var result = await _sut.GetClient("unknown", CancellationToken.None);

        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetClient_returns_client_for_known_id()
    {
        _store.GetById("my-client", Arg.Any<CancellationToken>())
            .Returns(new Client { ClientId = "my-client", ClientName = "My Client" });

        var result = await _sut.GetClient("my-client", CancellationToken.None);

        Assert.Contains("my-client", result);
        // The Secrets array must not appear in the serialised output.
        Assert.DoesNotContain("\"secrets\"", result, StringComparison.OrdinalIgnoreCase);
    }
}
