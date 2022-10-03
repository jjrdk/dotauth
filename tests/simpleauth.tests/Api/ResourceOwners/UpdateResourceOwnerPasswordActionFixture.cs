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

public sealed class UpdateResourceOwnerPasswordActionFixture
{
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub;

    public UpdateResourceOwnerPasswordActionFixture()
    {
        _resourceOwnerRepositoryStub = new InMemoryResourceOwnerRepository(string.Empty, new List<ResourceOwner>());
    }

    [Fact]
    public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
    {
        await Assert
            .ThrowsAsync<ArgumentNullException>(
                () => _resourceOwnerRepositoryStub.Update(null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Resource_Owner_Does_Not_Exist_Then_ReturnsFalse()
    {
        const string subject = "invalid_subject";

        var result = await _resourceOwnerRepositoryStub
            .Update(new ResourceOwner {Subject = subject}, CancellationToken.None)
            .ConfigureAwait(false);

        Assert.IsType<Option.Error>(result);
    }
}