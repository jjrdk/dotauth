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

namespace SimpleAuth.Api.PolicyController.Actions
{
    using Errors;
    using Extensions;
    using Repositories;
    using Shared.Models;
    using System;
    using System.Threading.Tasks;

    internal class GetAuthorizationPolicyAction : IGetAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;

        public GetAuthorizationPolicyAction(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<Policy> Execute(string policyId)
        {
            if (string.IsNullOrWhiteSpace(policyId))
            {
                throw new ArgumentNullException(nameof(policyId));
            }

            Policy policy = null;
            try
            {
                policy = await _policyRepository.Get(policyId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, policyId));
            }

            return policy;
        }
    }
}
