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

namespace SimpleAuth.Tests.Api.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Repositories;
    using Shared.Models;
    using Shared.Repositories;
    using Xunit;

    public class GetResourceOwnerActionFixture
    {
        private IResourceOwnerRepository _resourceOwnerRepository;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerRepository.Get(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Exist_Then_ReturnsNull()
        {
            const string subject = "invalid_subject";
            InitializeFakeObjects();

            var owner = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);

            Assert.Null(owner);
        }

        [Fact]
        public async Task When_Getting_Resource_Owner_Then_ResourceOwner_Is_Returned()
        {
            const string subject = "subject";
            InitializeFakeObjects(new ResourceOwner { Id = "subject" });

            var result = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);

            Assert.NotNull(result);
        }

        private void InitializeFakeObjects(params ResourceOwner[] resourceOwners)
        {
            _resourceOwnerRepository = new DefaultResourceOwnerRepository(resourceOwners);
        }
    }
}
