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
    using Repositories;
    using Shared.Models;
    using System;
    using System.Threading.Tasks;

    internal class DeleteResourcePolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;

        public DeleteResourcePolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentNullException(nameof(resourceId));
            }

            Policy policy;
            try
            {
                policy = await _policyRepository.Get(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                   string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, id),
                    ex);
            }

            if (policy == null)
            {
                return false;
            }

            ResourceSet resourceSet;
            try
            {
                resourceSet = await _resourceSetRepository.Get(resourceId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceId),
                    ex);
            }

            if (resourceSet == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidResourceSetId,
                    string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceId));
            }

            if (policy.ResourceSetIds == null ||
                !policy.ResourceSetIds.Contains(resourceId))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidResourceSetId,
                    ErrorDescriptions.ThePolicyDoesntContainResource);
            }

            policy.ResourceSetIds.Remove(resourceId);
            var result = await _policyRepository.Update(policy).ConfigureAwait(false);
            return result;
        }
    }
}
