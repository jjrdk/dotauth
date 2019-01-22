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
    using Parameters;
    using Repositories;
    using SimpleAuth.Api.PolicyController.Actions;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceSetAction.Execute(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_NoPolicyId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(() => _addResourceSetAction.Execute(new AddResourceSetParameter()))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddResourceSetParameterNames.PolicyId));
        }

        [Fact]
        public async Task When_Passing_NoResourceSetIds_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addResourceSetAction.Execute(
                    new AddResourceSetParameter
                    {
                        PolicyId = "policy_id"
                    }))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddResourceSetParameterNames.ResourceSet));
        }

        [Fact]
        public async Task When_One_ResourceSet_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string policyId = "policy_id";
            const string resourceSetId = "resource_set_id";
            InitializeFakeObjects(new Policy { Id = policyId });

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _addResourceSetAction.Execute(
                    new AddResourceSetParameter
                    {
                        PolicyId = policyId,
                        ResourceSets = new List<string>
                        {
                            resourceSetId
                        }
                    }))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidResourceSetId, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId), exception.Message);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Does_Not_Exist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            InitializeFakeObjects();

            var result = await _addResourceSetAction.Execute(new AddResourceSetParameter
            {
                PolicyId = policyId,
                ResourceSets = new List<string>
                    {
                        "resource_set_id"
                    }
            })
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
                    new AddResourceSetParameter
                    {
                        PolicyId = policyId,
                        ResourceSets = new List<string>
                        {
                            resourceSetId
                        }
                    })
                .ConfigureAwait(false);

            Assert.True(result);
        }

        private void InitializeFakeObjects(Policy policy = null, ResourceSet resourceSet = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(policy);
            _policyRepositoryStub.Setup(x => x.Update(It.IsAny<Policy>())).ReturnsAsync(true);
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _resourceSetRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(resourceSet);
            _addResourceSetAction = new AddResourceSetToPolicyAction(
                _policyRepositoryStub.Object,
                _resourceSetRepositoryStub.Object);
        }
    }
}
