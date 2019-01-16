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
    using Exceptions;
    using Extensions;
    using Parameters;
    using Repositories;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    internal class UpdatePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;

        public UpdatePolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(UpdatePolicyParameter updatePolicyParameter)
        {
            // Check the parameters
            if (updatePolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(updatePolicyParameter));
            }

            if (string.IsNullOrWhiteSpace(updatePolicyParameter.PolicyId))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }

            if (updatePolicyParameter.Rules == null || !updatePolicyParameter.Rules.Any())
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.Rules));
            }

            // Check the authorization policy exists.
            Policy policy = null;
            try
            {
                policy = await _policyRepository.Get(updatePolicyParameter.PolicyId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved));
            }

            if (policy == null)
            {
                return false;
            }

            policy.Rules = new List<PolicyRule>();
            // Check all the scopes are valid.
            foreach (var resourceSetId in policy.ResourceSetIds)
            {
                var resourceSet = await _resourceSetRepository.Get(resourceSetId).ConfigureAwait(false);
                if (updatePolicyParameter.Rules.Any(r =>
                    r.Scopes != null && !r.Scopes.All(s => resourceSet.Scopes.Contains(s))))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope,
                        ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            // Update the authorization policy.
            foreach (var ruleParameter in updatePolicyParameter.Rules)
            {
                var claims = new List<Claim>();
                if (ruleParameter.Claims != null)
                {
                    claims = ruleParameter.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                }

                policy.Rules.Add(new PolicyRule
                {
                    Id = ruleParameter.Id,
                    ClientIdsAllowed = ruleParameter.ClientIdsAllowed,
                    IsResourceOwnerConsentNeeded = ruleParameter.IsResourceOwnerConsentNeeded,
                    Scopes = ruleParameter.Scopes,
                    Script = ruleParameter.Script,
                    Claims = claims,
                    OpenIdProvider = ruleParameter.OpenIdProvider
                });
            }

            try
            {
                return await _policyRepository.Update(policy).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeUpdated,
                    updatePolicyParameter.PolicyId));
            }

            return true;
        }
    }
}
