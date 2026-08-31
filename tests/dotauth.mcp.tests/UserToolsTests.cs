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

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Mcp.Tools;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class UserToolsTests
{
    private readonly IResourceOwnerStore _store;
    private readonly UserTools _sut;

    public UserToolsTests()
    {
        _store = Substitute.For<IResourceOwnerStore>();
        _sut = new UserTools(_store);
    }

    [Fact]
    public async Task GetUser_returns_not_found_for_unknown_subject()
    {
        _store.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ResourceOwner?)null);

        var result = await _sut.GetUser("alice", CancellationToken.None);

        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUser_never_returns_password()
    {
        _store.Get("alice", Arg.Any<CancellationToken>())
            .Returns(new ResourceOwner
            {
                Subject = "alice",
                Password = "s3cr3t",
                Claims = [new Claim("sub", "alice"), new Claim("email", "alice@example.com")]
            });

        var result = await _sut.GetUser("alice", CancellationToken.None);

        Assert.Contains("alice", result);
        Assert.DoesNotContain("s3cr3t", result);
    }

    [Fact]
    public async Task ListUsers_returns_listing_not_supported_when_store_is_not_repository()
    {
        // The base IResourceOwnerStore does not expose GetAll; only IResourceOwnerRepository does.
        var result = await _sut.ListUsers(CancellationToken.None);

        Assert.Contains("not supported", result, StringComparison.OrdinalIgnoreCase);
    }
}
