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

namespace SimpleAuth.Uma.Tests.Api.ResourceSetController.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Repositories;
    using SimpleAuth.Api.ResourceSetController;
    using SimpleAuth.Shared.Models;
    using Xunit;

    public class GetPoliciesActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private GetPoliciesAction _getPoliciesAction;

        [Fact]
        public async Task When_Passing_NullOrEmpty_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getPoliciesAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getPoliciesAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_RetrievingPolicies_Then_Ids_Are_Returned()
        {
            const string policyId = "policy_id";
            ICollection<Policy> policies = new List<Policy>
            {
                new Policy
                {
                    Id = policyId
                }
            };
            InitializeFakeObjects();
            _policyRepositoryStub.Setup(p => p.SearchByResourceId(It.IsAny<string>()))
                .Returns(Task.FromResult(policies));

            var result = await _getPoliciesAction.Execute("resource_id").ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result.Count() == 1);
            Assert.True(result.First() == policyId);
        }

        private void InitializeFakeObjects()
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _getPoliciesAction = new GetPoliciesAction(_policyRepositoryStub.Object);
        }
    }
}
