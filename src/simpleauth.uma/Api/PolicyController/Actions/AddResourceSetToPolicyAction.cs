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
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Helpers;
    using Logging;
    using Parameters;
    using Repositories;
    using SimpleAuth.Errors;
    using ErrorDescriptions = Errors.ErrorDescriptions;

    internal class AddResourceSetToPolicyAction : IAddResourceSetToPolicyAction
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AddResourceSetToPolicyAction(
            IPolicyRepository policyRepository,
            IResourceSetRepository resourceSetRepository,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaServerEventSource umaServerEventSource)
        {
            _policyRepository = policyRepository;
            _resourceSetRepository = resourceSetRepository;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(AddResourceSetParameter addResourceSetParameter)
        {
            if (addResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(addResourceSetParameter));
            }

            if (string.IsNullOrWhiteSpace(addResourceSetParameter.PolicyId))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddResourceSetParameterNames.PolicyId));
            }

            if (addResourceSetParameter.ResourceSets == null  ||
                !addResourceSetParameter.ResourceSets.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.AddResourceSetParameterNames.ResourceSet));
            }

            _umaServerEventSource.StartAddResourceToAuthorizationPolicy(addResourceSetParameter.PolicyId, string.Join(",", addResourceSetParameter.ResourceSets));
            var policy = await _repositoryExceptionHelper.HandleException(
                string.Format(ErrorDescriptions.TheAuthorizationPolicyCannotBeRetrieved, addResourceSetParameter.PolicyId),
                () => _policyRepository.Get(addResourceSetParameter.PolicyId)).ConfigureAwait(false);
            if (policy == null)
            {
                return false;
            }
                        
            foreach (var resourceSetId in addResourceSetParameter.ResourceSets)
            {
                var resourceSet = await _repositoryExceptionHelper.HandleException(
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRetrieved, resourceSetId),
                    () => _resourceSetRepository.Get(resourceSetId)).ConfigureAwait(false);
                if (resourceSet == null)
                {
                    throw new BaseUmaException(UmaErrorCodes.InvalidResourceSetId, string.Format(ErrorDescriptions.TheResourceSetDoesntExist, resourceSetId));
                }
            }

            policy.ResourceSetIds.AddRange(addResourceSetParameter.ResourceSets);
            var result = await _repositoryExceptionHelper.HandleException(
                ErrorDescriptions.ThePolicyCannotBeUpdated,
                () => _policyRepository.Update(policy)).ConfigureAwait(false);
            _umaServerEventSource.FinishAddResourceToAuthorizationPolicy(addResourceSetParameter.PolicyId, string.Join(",", addResourceSetParameter.ResourceSets));
            return result;
        }
    }
}
