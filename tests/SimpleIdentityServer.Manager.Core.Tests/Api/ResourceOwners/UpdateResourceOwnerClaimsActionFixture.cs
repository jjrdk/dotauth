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

namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    using Moq;
    using SimpleIdentityServer.Core.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class UpdateResourceOwnerClaimsActionFixture
    {
        private IResourceOwnerRepository _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerRepositoryStub.UpdateAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Exist_Then_ReturnsNull()
        {
            const string subject = "invalid_subject";

            InitializeFakeObjects();

            var owner = await _resourceOwnerRepositoryStub.Get(subject).ConfigureAwait(false);

            Assert.Null(owner);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Updated_Then_ReturnsFalse()
        {
            InitializeFakeObjects();

            var result = await _resourceOwnerRepositoryStub.UpdateAsync(new ResourceOwner { Id = "blah" })
                .ConfigureAwait(false);

            Assert.False(result);
        }

        private void InitializeFakeObjects(params ResourceOwner[] resourceOwners)
        {
            _resourceOwnerRepositoryStub = new DefaultResourceOwnerRepository(new List<ResourceOwner>(resourceOwners));
            _claimRepositoryStub = new Mock<IClaimRepository>();
        }
    }
}
