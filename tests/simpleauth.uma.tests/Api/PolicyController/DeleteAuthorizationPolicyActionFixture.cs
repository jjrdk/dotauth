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
    using Helpers;
    using Models;
    using Moq;
    using Repositories;
    using System;
    using System.Threading.Tasks;
    using Uma.Api.PolicyController.Actions;
    using Xunit;

    public class DeleteAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private Mock<IRepositoryExceptionHelper> _repositoryExceptionHelperStub;
        private IDeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;

        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            IntializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteAuthorizationPolicyAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Does_Not_Exist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(() => Task.FromResult((Policy)null));

            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId).ConfigureAwait(false);

            Assert.False(isUpdated);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Exists_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            var policy = new Policy();
            IntializeFakeObjects();
            _repositoryExceptionHelperStub.Setup(r => r.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId),
                It.IsAny<Func<Task<Policy>>>()))
                .Returns(Task.FromResult(policy));

            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId).ConfigureAwait(false);

            Assert.True(isUpdated);
        }

        private void IntializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _repositoryExceptionHelperStub = new Mock<IRepositoryExceptionHelper>();
            _deleteAuthorizationPolicyAction = new DeleteAuthorizationPolicyAction(
                _policyRepositoryStub.Object,
                _repositoryExceptionHelperStub.Object);
        }
    }
}
