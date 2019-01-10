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
    using Models;
    using Moq;
    using Repositories;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Uma.Api.PolicyController.Actions;
    using Xunit;

    public class GetAuthorizationPoliciesActionFixture
    {
        private Mock<IPolicyRepository> _policyRepositoryStub;
        private IGetAuthorizationPoliciesAction _getAuthorizationPoliciesAction;

        [Fact]
        public async Task When_Getting_Authorization_Policies_Then_A_ListIds_Is_Returned()
        {
            const string policyId = "policy_id";
            ICollection<Policy> policies = new List<Policy>
            {
                new Policy
                {
                    Id = policyId
                }
            };

            InitializeFakeObjects(policies);

            var result = await _getAuthorizationPoliciesAction.Execute().ConfigureAwait(false);

            Assert.Single(result);
            Assert.Equal(policyId, result.First());
        }

        private void InitializeFakeObjects(IEnumerable<Policy> policies)
        {
            _policyRepositoryStub = new Mock<IPolicyRepository>();
            _policyRepositoryStub.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(policies.FirstOrDefault);
            _policyRepositoryStub.Setup(x => x.GetAll()).ReturnsAsync(policies.ToList);
            _getAuthorizationPoliciesAction = new GetAuthorizationPoliciesAction(_policyRepositoryStub.Object);
        }
    }
}
