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

namespace SimpleAuth.Uma.Api.PolicyController.Actions
{
    using Errors;
    using Exceptions;
    using Helpers;
    using Logging;
    using Models;
    using Parameters;
    using Repositories;
    using SimpleAuth.Errors;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using ErrorDescriptions = Errors.ErrorDescriptions;

    internal class AddAuthorizationPolicyAction : IAddAuthorizationPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;

        public AddAuthorizationPolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
        }

        public async Task<string> Execute(AddPolicyParameter addPolicyParameter)
        {
            if (addPolicyParameter == null)
            {
                throw new ArgumentNullException(nameof(addPolicyParameter));
            }

            if (addPolicyParameter.ResourceSetIds == null || !addPolicyParameter.ResourceSetIds.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.ResourceSetIds));
            }

            if (addPolicyParameter.Rules == null || !addPolicyParameter.Rules.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        UmaConstants.AddPolicyParameterNames.Rules));
            }

            foreach (var resourceSetId in addPolicyParameter.ResourceSetIds)
            {
                var resourceSet = await _repositoryExceptionHelper.HandleException(
                        string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId),
                        () => _resourceSetRepository.Get(resourceSetId))
                    .ConfigureAwait(false);
                if (resourceSet == null)
                {
                    throw new BaseUmaException(UmaErrorCodes.InvalidResourceSetId,
                        string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }

                if (addPolicyParameter.Rules.Any(r =>
                    r.Scopes != null && !r.Scopes.All(s => resourceSet.Scopes.Contains(s))))
                {
                    throw new BaseUmaException(UmaErrorCodes.InvalidScope,
                        ErrorDescriptions.OneOrMoreScopesDontBelongToAResourceSet);
                }
            }

            var rules = new List<PolicyRule>();
            foreach (var ruleParameter in addPolicyParameter.Rules)
            {
                var claims = new List<Claim>();
                if (ruleParameter.Claims != null)
                {
                    claims = ruleParameter.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                }

                rules.Add(new PolicyRule
                {
                    Id = Id.Create(),
                    IsResourceOwnerConsentNeeded = ruleParameter.IsResourceOwnerConsentNeeded,
                    ClientIdsAllowed = ruleParameter.ClientIdsAllowed,
                    Scopes = ruleParameter.Scopes,
                    Script = ruleParameter.Script,
                    Claims = claims,
                    OpenIdProvider = ruleParameter.OpenIdProvider
                });
            }

            // Insert policy
            var policy = new Policy
            {
                Id = Id.Create(),
                Rules = rules,
                ResourceSetIds = addPolicyParameter.ResourceSetIds
            };

            await _repositoryExceptionHelper.HandleException(
                    ErrorDescriptions.ThePolicyCannotBeInserted,
                    () => _policyRepository.Add(policy))
                .ConfigureAwait(false);
            return policy.Id;
        }
    }
}
