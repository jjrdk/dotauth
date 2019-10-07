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
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class AddResourceSetToPolicyAction
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

        public async Task<bool> Execute(
            AddResourceSetParameter addResourceSetParameter,
            CancellationToken cancellationToken)
        {
            if (addResourceSetParameter.ResourceSets == null || addResourceSetParameter.ResourceSets.Length == 0)
            {
                return false;
            }

            var policy = await _policyRepository.Get(addResourceSetParameter.PolicyId, cancellationToken)
                .ConfigureAwait(false);

            if (policy == null)
            {
                return false;
            }

            foreach (var resourceSetId in addResourceSetParameter.ResourceSets)
            {
                var resourceSet = await _resourceSetRepository.Get(resourceSetId, cancellationToken).ConfigureAwait(false);

                if (resourceSet == null)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidResourceSetId,
                        string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }
            }

            policy.ResourceSetIds = policy.ResourceSetIds.Concat(addResourceSetParameter.ResourceSets).ToArray();

            try
            {
                return await _policyRepository.Update(policy, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(ErrorCodes.InternalError, ErrorDescriptions.ThePolicyCannotBeUpdated, ex);
            }
        }
    }
}
