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

namespace SimpleAuth.Uma.Tests.Api.PolicyController
{
    using Errors;
    using Exceptions;
    using Moq;
    using Repositories;
    using SimpleAuth.Api.PolicyController.Actions;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class DeleteResourcePolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private DeleteResourcePolicyAction _deleteResourcePolicyAction;

        [Fact]
        public async Task When_Passing_NullOrEmpty_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(null, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute(string.Empty, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _deleteResourcePolicyAction.Execute("policy_id", null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _deleteResourcePolicyAction.Execute("policy_id", string.Empty))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceDoesntExist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";
            InitializeFakeObjects(new Policy());

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId))
                .ConfigureAwait(false);

            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
        }

        [Fact]
        public async Task When_PolicyDoesntContainResource_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceId = "invalid_resource_id";
            var policy = new Policy
            {
                ResourceSetIds = new List<string>
                {
                    "resource_id"
                }
            };
            InitializeFakeObjects(policy, new ResourceSet());

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _deleteResourcePolicyAction.Execute(policyId, resourceId))
                .ConfigureAwait(false);

            Assert.True(exception.Code == ErrorCodes.InvalidResourceSetId);
            Assert.True(exception.Message == ErrorDescriptions.ThePolicyDoesntContainResource);
        }

        [Fact]
        public async Task When_AuthorizationPolicyDoesntExist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            InitializeFakeObjects();

            var result = await _deleteResourcePolicyAction.Execute(policyId, "resource_id").ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceIsRemovedFromPolicy_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            const string resourceId = "resource_id";

            var policy = new Policy
            {
                ResourceSetIds = new List<string>
                {
                    resourceId
                }
            };
            InitializeFakeObjects(policy, new ResourceSet());

            _policyRepositoryStub.Setup(p => p.Update(It.IsAny<Policy>())).ReturnsAsync(true);

            var result = await _deleteResourcePolicyAction.Execute(policyId, resourceId).ConfigureAwait(false);

            Assert.True(result);
        }

        private void InitializeFakeObjects(Policy policy = null, ResourceSet resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(policy);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(resourceSet);
            _deleteResourcePolicyAction = new DeleteResourcePolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
