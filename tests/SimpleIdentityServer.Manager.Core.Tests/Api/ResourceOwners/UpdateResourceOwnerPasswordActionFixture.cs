namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    using SimpleIdentityServer.Core.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class UpdateResourceOwnerPasswordActionFixture
    {
        private IResourceOwnerRepository _resourceOwnerRepositoryStub;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerRepositoryStub.UpdateAsync(null))
    .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Doesnt_Exist_Then_ReturnsFalse()
        {
            const string subject = "invalid_subject";
            InitializeFakeObjects();

            var result = await _resourceOwnerRepositoryStub.UpdateAsync(new ResourceOwner { Id = subject }).ConfigureAwait(false);

            Assert.False(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new DefaultResourceOwnerRepository(new List<ResourceOwner>());
        }
    }
}
