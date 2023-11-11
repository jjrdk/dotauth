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

namespace DotAuth.Tests.Api.ResourceOwners;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Xunit;

public sealed class UpdateResourceOwnerClaimsActionFixture
{
    private IResourceOwnerRepository _resourceOwnerRepositoryStub;

    [Fact]
    public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
    {
        InitializeFakeObjects();

        await Assert
            .ThrowsAsync<ArgumentNullException>(
                () => _resourceOwnerRepositoryStub.Update(null, CancellationToken.None))
            ;
    }

    [Fact]
    public async Task When_ResourceOwner_Does_Not_Exist_Then_ReturnsNull()
    {
        const string subject = "invalid_subject";

        InitializeFakeObjects();

        var owner = await _resourceOwnerRepositoryStub.Get(subject, CancellationToken.None);

        Assert.Null(owner);
    }

    [Fact]
    public async Task When_Resource_Owner_Cannot_Be_Updated_Then_ReturnsFalse()
    {
        InitializeFakeObjects();

        var result = await _resourceOwnerRepositoryStub
            .Update(new ResourceOwner { Subject = "blah" }, CancellationToken.None)
            ;

        Assert.IsType<Option.Error>(result);
    }

    private void InitializeFakeObjects(params ResourceOwner[] resourceOwners)
    {
        _resourceOwnerRepositoryStub = new InMemoryResourceOwnerRepository(string.Empty, new List<ResourceOwner>(resourceOwners));
    }
}
