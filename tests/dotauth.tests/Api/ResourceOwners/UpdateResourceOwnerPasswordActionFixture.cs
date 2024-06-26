﻿namespace DotAuth.Tests.Api.ResourceOwners;

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
    private readonly IResourceOwnerRepository _resourceOwnerRepositoryStub = new InMemoryResourceOwnerRepository(string.Empty, new List<ResourceOwner>());

    [Fact]
    public async Task When_Resource_Owner_Does_Not_Exist_Then_ReturnsFalse()
    {
        const string subject = "invalid_subject";

        var result = await _resourceOwnerRepositoryStub
            .Update(new ResourceOwner {Subject = subject}, CancellationToken.None)
            ;

        Assert.IsType<Option.Error>(result);
    }
}
