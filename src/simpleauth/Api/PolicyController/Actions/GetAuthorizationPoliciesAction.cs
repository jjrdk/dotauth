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
    using Repositories;
    using Shared.Models;
    using SimpleAuth.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class GetAuthorizationPoliciesAction
    {
        private readonly IPolicyRepository _policyRepository;

        public GetAuthorizationPoliciesAction(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<ICollection<string>> Execute()
        {
            ICollection<Policy> policies = null;
            try
            {
                policies = await _policyRepository.GetAll().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved,
                    ex);
            }

            return policies?.Select(p => p.Id).ToList();
        }
    }
}
