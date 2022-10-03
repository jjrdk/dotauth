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

namespace DotAuth.Server.Tests.Apis;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.ResourceSetController;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public sealed class UpdateResourceSetActionFixture
{
    private readonly Mock<IResourceSetRepository> _resourceSetRepositoryStub;
    private readonly UpdateResourceSetAction _updateResourceSetAction;

    public UpdateResourceSetActionFixture()
    {
        _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
        _resourceSetRepositoryStub.Setup(x => x.Update(It.IsAny<ResourceSet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Option.Success());
        _updateResourceSetAction = new UpdateResourceSetAction(_resourceSetRepositoryStub.Object, new Mock<ILogger>().Object);
    }

    [Fact]
    public async Task When_Passing_No_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _updateResourceSetAction.Execute(null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_ResourceSet_Cannot_Be_Updated_Then_Returns_False()
    {
        const string id = "id";
        var udpateResourceSetParameter = new ResourceSet
        {
            Id = id,
            Name = "blah",
            Scopes = new[] { "scope" }
        };
        var resourceSet = new Shared.Models.ResourceSet { Id = id };
        _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceSet);
        _resourceSetRepositoryStub.Setup(r => r.Update(It.IsAny<ResourceSet>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<Option>(new Option.Error(new ErrorDetails())));

        var result = await _updateResourceSetAction.Execute(udpateResourceSetParameter, CancellationToken.None).ConfigureAwait(false);
        Assert.IsType<Option.Error>(result);
    }

    [Fact]
    public async Task When_A_ResourceSet_Is_Updated_Then_True_Is_Returned()
    {
        const string id = "id";
        var udpateResourceSetParameter = new ResourceSet
        {
            Id = id,
            Name = "blah",
            Scopes = new[] { "scope" }
        };
        var resourceSet = new Shared.Models.ResourceSet { Id = id };
        _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceSet);
        _resourceSetRepositoryStub.Setup(r => r.Update(It.IsAny<Shared.Models.ResourceSet>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Option.Success());

        var result = await _updateResourceSetAction.Execute(udpateResourceSetParameter, CancellationToken.None)
            .ConfigureAwait(false);

        Assert.IsType<Option.Success>(result);
    }
}