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

namespace SimpleAuth.Uma.Api.ResourceSetController.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Logging;
    using Models;
    using Newtonsoft.Json;
    using Parameters;
    using Repositories;
    using Validators;

    internal class UpdateResourceSetAction : IUpdateResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IResourceSetParameterValidator _resourceSetParameterValidator;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public UpdateResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IResourceSetParameterValidator resourceSetParameterValidator,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _resourceSetParameterValidator = resourceSetParameterValidator;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<bool> Execute(UpdateResourceSetParameter udpateResourceSetParameter)
        {
            if (udpateResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(udpateResourceSetParameter));
            }

            var json = JsonConvert.SerializeObject(udpateResourceSetParameter);
            _umaServerEventSource.StartToUpdateResourceSet(json);
            var resourceSet = new ResourceSet
            {
                Id = udpateResourceSetParameter.Id,
                Name = udpateResourceSetParameter.Name,
                Uri = udpateResourceSetParameter.Uri,
                Type = udpateResourceSetParameter.Type,
                Scopes = udpateResourceSetParameter.Scopes,
                IconUri = udpateResourceSetParameter.IconUri
            };

            if (string.IsNullOrWhiteSpace(udpateResourceSetParameter.Id))
            {
                throw new BaseUmaException(UmaErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }
            _resourceSetParameterValidator.CheckResourceSetParameter(resourceSet);
            if (await _resourceSetRepository.Get(udpateResourceSetParameter.Id).ConfigureAwait(false) == null)
            {
                return false;
            }

            if (!await _resourceSetRepository.Update(resourceSet).ConfigureAwait(false))
            {
                throw new BaseUmaException(UmaErrorCodes.InternalError, string.Format(ErrorDescriptions.TheResourceSetCannotBeUpdated, resourceSet.Id));
            }

            _umaServerEventSource.FinishToUpdateResourceSet(json);
            return true;
        }
    }
}
