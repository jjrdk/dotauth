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

namespace SimpleAuth.Tests.WebSite.User
{
    using Moq;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.WebSite.User.Actions;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Events.Logging;
    using Xunit;

    public class AddUserOperationFixture
    {
        private readonly Mock<IEventPublisher> _eventPublisher;
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly AddUserOperation _addResourceOwnerAction;

        public AddUserOperationFixture()
        {
            _eventPublisher = new Mock<IEventPublisher>();
            _eventPublisher.Setup(s => s.Publish(It.IsAny<ResourceOwnerAdded>())).Returns(Task.CompletedTask);
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _addResourceOwnerAction = new AddUserOperation(
                _resourceOwnerRepositoryStub.Object,
                Array.Empty<IAccountFilter>(),
                _eventPublisher.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert
                .ThrowsAsync<NullReferenceException>(() => _addResourceOwnerAction.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _addResourceOwnerAction.Execute(new ResourceOwner(), CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_With_Same_Credentials_Exists_Then_Returns_False()
        {
            var parameter = new ResourceOwner {Subject = "name", Password = "password"};

            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceOwner());

            var result = await _addResourceOwnerAction.Execute(parameter, CancellationToken.None).ConfigureAwait(false);
            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceOwner_Cannot_Be_Added_Then_Returns_False()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var parameter = new ResourceOwner {Subject = "name", Password = "password"};

            var result = await _addResourceOwnerAction.Execute(parameter, CancellationToken.None).ConfigureAwait(false);
            Assert.False(result);
        }

        [Fact]
        public async Task When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            var parameter = new ResourceOwner {Subject = "name", Password = "password"};

            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceOwner) null);
            _resourceOwnerRepositoryStub.Setup(r => r.Insert(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _addResourceOwnerAction.Execute(parameter, CancellationToken.None).ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Verify(
                r => r.Insert(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()));
            _eventPublisher.Verify(o => o.Publish(It.IsAny<ResourceOwnerAdded>()));
        }
    }
}
