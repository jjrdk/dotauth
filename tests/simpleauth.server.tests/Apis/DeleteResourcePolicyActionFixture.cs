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

namespace SimpleAuth.Server.Tests.Apis
{
    using Moq;
    using SimpleAuth.Api.PolicyController;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class DeleteResourcePolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private DeleteResourcePolicyAction _deleteResourcePolicyAction;

        [Fact]
        public async Task WhenPassingNullIdAndNullResourceIdThenReturnsFalse()
        {
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute(null, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task WhenPassingEmptyIdAndNullResourceIdThenReturnsFalse()
        {
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute(string.Empty, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task WhenPassingValidIdAndNullResourceIdThenReturnsFalse()
        {
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute("policy_id", null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task WhenPassingValidIdAndEmptyResourceIdThenReturnsFalse()
        {
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute("policy_id", string.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceDoesntExist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";
            InitializeFakeObjects(new Policy());

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _deleteResourcePolicyAction.Execute(policyId, resourceId, CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId), exception.Message);
        }

        [Fact]
        public async Task When_PolicyDoesntContainResource_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "invalid_resource_id";
            var policy = new Policy { ResourceSetIds = new[] { "resource_id" } };
            InitializeFakeObjects(policy, new ResourceSetModel());

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _deleteResourcePolicyAction.Execute(policyId, resourceId, CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(ErrorDescriptions.ThePolicyDoesntContainResource, exception.Message);
        }

        [Fact]
        public async Task When_AuthorizationPolicyDoesntExist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute(policyId, "resource_id", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceIsRemovedFromPolicy_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            var policy = new Policy { ResourceSetIds = new[] { resourceId } };
            InitializeFakeObjects(policy, new ResourceSetModel());

            _policyRepositoryStub.Setup(p => p.Update(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _deleteResourcePolicyAction.Execute(policyId, resourceId, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.True(result);
        }

        private void InitializeFakeObjects(Policy policy = null, ResourceSetModel resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resourceSet);
            _deleteResourcePolicyAction = new DeleteResourcePolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
