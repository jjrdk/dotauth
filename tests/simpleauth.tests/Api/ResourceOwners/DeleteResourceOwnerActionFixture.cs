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
    using System.Threading;
    using System.Threading.Tasks;
    using Repositories;
    using Shared.Models;
    using Shared.Repositories;
    using Xunit;

    public class DeleteResourceOwnerActionFixture
    {
        private IResourceOwnerRepository _resourceOwnerRepositoryStub;

        [Fact]
        public async Task When_ResourceOwner_Does_Not_Exist_Then_ReturnsFalse()
        {
            const string subject = "invalid_subject";
            InitializeFakeObjects();

            var result = await _resourceOwnerRepositoryStub.Delete(subject, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_Cannot_Delete_Resource_Owner_Then_ReturnsFalse()
        {
            const string subject = "subject";
            InitializeFakeObjects(new ResourceOwner());

            var result = await _resourceOwnerRepositoryStub.Delete(subject, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        private void InitializeFakeObjects(params ResourceOwner[] resourceOwners)
        {
            _resourceOwnerRepositoryStub = new InMemoryResourceOwnerRepository(resourceOwners);
        }
    }
}
