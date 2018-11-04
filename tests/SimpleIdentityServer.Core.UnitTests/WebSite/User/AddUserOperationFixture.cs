// Copyright 2015 Habart Thierry
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

using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    using System.Threading;
    using Logging;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;

    public class AddUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimsRepositoryStub;
        private Mock<IAccessTokenStore> _tokenStoreStub;
        //private Mock<IScimClientFactory> _scimClientFactoryStub;
        private Mock<ILinkProfileAction> _linkProfileActionStub;
        private Mock<IOpenIdEventSource> _openidEventSourceStub;
        private IAddUserOperation _addResourceOwnerAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new ResourceOwner())).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new ResourceOwner { Id = "test" })).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_With_Same_Credentials_Exists_Then_Returns_False()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new ResourceOwner { Id = "name", Password = "password" };

            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            var result = await _addResourceOwnerAction.Execute(parameter).ConfigureAwait(false);
            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceOwner_Cannot_Be_Added_Then_Returns_False()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>()))
                .Returns(Task.FromResult(false));
            var parameter = new ResourceOwner
            {
                Id = "name",
                Password = "password"
            };

            var result = await _addResourceOwnerAction.Execute(parameter).ConfigureAwait(false);
            Assert.False(result);
        }

        [Fact]
        public async Task When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            //var parameter = new AddUserParameter("name", "password", new List<Claim>());
            var parameter = new ResourceOwner
            {
                Id = "name",
                Password = "password"
            };

            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ResourceOwner)null));
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(true));

            // ACT
            await _addResourceOwnerAction.Execute(parameter).ConfigureAwait(false);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.InsertAsync(It.IsAny<ResourceOwner>()));
            _openidEventSourceStub.Verify(o => o.AddResourceOwner("name"));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimsRepositoryStub = new Mock<IClaimRepository>();
            _tokenStoreStub = new Mock<IAccessTokenStore>();
            _linkProfileActionStub = new Mock<ILinkProfileAction>();
            _openidEventSourceStub = new Mock<IOpenIdEventSource>();
            _addResourceOwnerAction = new AddUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _claimsRepositoryStub.Object,
                null,
                _openidEventSourceStub.Object);
        }
    }
}
