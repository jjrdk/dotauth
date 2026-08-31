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

public sealed class ScopeToolsTests
{
    private readonly IScopeStore _store;
    private readonly ScopeTools _sut;

    public ScopeToolsTests()
    {
        _store = Substitute.For<IScopeStore>();
        _sut = new ScopeTools(_store);
    }

    [Fact]
    public async Task ListScopes_returns_all_scopes()
    {
        _store.GetAll(Arg.Any<CancellationToken>())
            .Returns(
            [
                new Scope { Name = "openid", Description = "OpenID scope" },
                new Scope { Name = "profile", Description = "Profile scope" }
            ]);

        var result = await _sut.ListScopes(CancellationToken.None);

        Assert.Contains("openid", result);
        Assert.Contains("profile", result);
    }

    [Fact]
    public async Task GetScope_returns_not_found_for_unknown_scope()
    {
        _store.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Scope?)null);

        var result = await _sut.GetScope("unknown", CancellationToken.None);

        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetScope_returns_scope_for_known_name()
    {
        _store.Get("openid", Arg.Any<CancellationToken>())
            .Returns(new Scope { Name = "openid", Description = "OpenID scope" });

        var result = await _sut.GetScope("openid", CancellationToken.None);

        Assert.Contains("openid", result);
        Assert.Contains("OpenID scope", result);
    }
}
