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

namespace SimpleAuth.Api.PolicyController
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class AddAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;

        public AddAuthorizationPolicyAction(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<string> Execute(string owner, PolicyData addPolicyParameter, CancellationToken cancellationToken)
        {
            if (addPolicyParameter.Rules == null || addPolicyParameter.Rules.Length == 0)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.Rules));
            }

            var rules = addPolicyParameter.Rules.Select(
                    ruleParameter => new PolicyRule
                    {
                        IsResourceOwnerConsentNeeded = ruleParameter.IsResourceOwnerConsentNeeded,
                        ClientIdsAllowed = ruleParameter.ClientIdsAllowed,
                        Scopes = ruleParameter.Scopes,
                        Script = ruleParameter.Script,
                        Claims = ruleParameter.Claims == null
                            ? Array.Empty<Claim>()
                            : ruleParameter.Claims.Select(c => new Claim(c.Type, c.Value)).ToArray(),
                        OpenIdProvider = ruleParameter.OpenIdProvider
                    })
                .ToArray();

            // Insert policy
            var policy = new Policy
            {
                Id = Id.Create(),
                Owner = owner,
                Rules = rules,
            };

            try
            {
                var result = await _policyRepository.Add(policy, cancellationToken).ConfigureAwait(false);

                return result ? policy.Id : null;
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.ThePolicyCannotBeInserted,
                    ex);
            }
        }
    }
}
