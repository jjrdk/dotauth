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
        private readonly IResourceSetRepository _resourceSetRepository;

        public AddAuthorizationPolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<string> Execute(PostPolicy addPolicyParameter, CancellationToken cancellationToken)
        {
            if (addPolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(addPolicyParameter));
            }

            if (addPolicyParameter.ResourceSetIds == null || !addPolicyParameter.ResourceSetIds.Any())
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.ResourceSetIds));
            }

            if (addPolicyParameter.Rules == null || !addPolicyParameter.Rules.Any())
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.Rules));
            }

            foreach (var resourceSetId in addPolicyParameter.ResourceSetIds)
            {
                ResourceSet resourceSet;
                try
                {
                    resourceSet = await _resourceSetRepository.Get(resourceSetId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InternalError,
                        string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId),
                        ex);
                }

                if (resourceSet == null)
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidResourceSetId,
                        string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }

                if (addPolicyParameter.Rules.Any(r =>
                    r.Scopes != null && !r.Scopes.All(s => resourceSet.Scopes.Contains(s))))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope,
                        ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            var rules = addPolicyParameter.Rules.Select(
                    ruleParameter => new PolicyRule
                    {
                        Id = Id.Create(),
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
                Rules = rules,
                ResourceSetIds = addPolicyParameter.ResourceSetIds
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
