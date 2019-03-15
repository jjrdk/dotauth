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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Api.PolicyController;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class AddResourceSetToPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private AddResourceSetToPolicyAction _addResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_NoPolicyId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addResourceSetAction.Execute(new AddResourceSetParameter(), CancellationToken.None))
                .ConfigureAwait(false);
            
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheParameterNeedsToBeSpecified,
                    UmaConstants.AddResourceSetParameterNames.PolicyId),
                exception.Message);
        }

        [Fact]
        public async Task When_Passing_NoResourceSetIds_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addResourceSetAction.Execute(
                        new AddResourceSetParameter { PolicyId = "policy_id" },
                        CancellationToken.None))
                .ConfigureAwait(false);
            
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheParameterNeedsToBeSpecified,
                    UmaConstants.AddResourceSetParameterNames.ResourceSet),
                exception.Message);
        }

        [Fact]
        public async Task When_One_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(new Policy { Id = policyId });

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _addResourceSetAction.Execute(
                        new AddResourceSetParameter { PolicyId = policyId, ResourceSets = new[] { resourceSetId } },
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId), exception.Message);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Does_Not_Exist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            InitializeFakeObjects();

            var result = await _addResourceSetAction.Execute(
                    new AddResourceSetParameter { PolicyId = policyId, ResourceSets = new[] { "resource_set_id" } },
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceSet_Is_Inserted_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(new Policy { Id = policyId }, new ResourceSet { Id = resourceSetId });

            var result = await _addResourceSetAction.Execute(
                    new AddResourceSetParameter { PolicyId = policyId, ResourceSets = new[] { resourceSetId } },
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.True(result);
        }

        private void InitializeFakeObjects(Policy policy = null, ResourceSet resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);
            _policyRepositoryStub.Setup(x => x.Update(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resourceSet);
            _addResourceSetAction = new AddResourceSetToPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
