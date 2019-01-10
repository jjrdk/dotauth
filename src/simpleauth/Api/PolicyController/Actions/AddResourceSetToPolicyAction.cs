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
    using System.Linq;
    using System.Threading.Tasks;

    internal class AddResourceSetToPolicyAction : IAddResourceSetToPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;

        public AddResourceSetToPolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(AddResourceSetParameter addResourceSetParameter)
        {
            if (addResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(addResourceSetParameter));
            }

            if (string.IsNullOrWhiteSpace(addResourceSetParameter.PolicyId))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddResourceSetParameterNames.PolicyId));
            }

            if (addResourceSetParameter.ResourceSets == null ||
                !addResourceSetParameter.ResourceSets.Any())
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddResourceSetParameterNames.ResourceSet));
            }

            Policy policy = null;
            try
            {
                policy = await _policyRepository.Get(addResourceSetParameter.PolicyId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.HandleException(string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, addResourceSetParameter.PolicyId));
            }

            if (policy == null)
            {
                return false;
            }

            foreach (var resourceSetId in addResourceSetParameter.ResourceSets)
            {
                ResourceSet resourceSet = null;
                try
                {
                    resourceSet = await _resourceSetRepository.Get(resourceSetId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ex.HandleException(string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId));
                }

                if (resourceSet == null)
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidResourceSetId, string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }
            }

            policy.ResourceSetIds.AddRange(addResourceSetParameter.ResourceSets);
            var result = false;
            try
            {
                result = await _policyRepository.Update(policy).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex.HandleException(ErrorDescriptions.ThePolicyCannotBeUpdated);
            }
            return result;
        }
    }
}
