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
    using SimpleAuth.Api.PolicyController.Actions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using Xunit;

    public class DeleteAuthorizationPolicyActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private DeleteAuthorizationPolicyAction _deleteAuthorizationPolicyAction;

        [Fact]
        public async Task When_Passing_Empty_Parameter_Then_Exception_Is_Thrown()
        {
            IntializeFakeObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _deleteAuthorizationPolicyAction.Execute(null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Does_Not_Exist_Then_False_Is_Returned()
        {
            const string policyId = "policy_id";
            IntializeFakeObjects();

            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.False(isUpdated);
        }

        [Fact]
        public async Task When_AuthorizationPolicy_Exists_Then_True_Is_Returned()
        {
            const string policyId = "policy_id";
            IntializeFakeObjects(new Policy {Id = policyId});

            var isUpdated = await _deleteAuthorizationPolicyAction.Execute(policyId, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.True(isUpdated);
        }

        private void IntializeFakeObjects(Policy policy = null)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);
            _deleteAuthorizationPolicyAction = new DeleteAuthorizationPolicyAction(_policyRepositoryStub.Object);
        }
    }
}
