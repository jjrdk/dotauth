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

    internal class AddResourceSetAction : IAddResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IResourceSetParameterValidator _resourceSetParameterValidator;

        public AddResourceSetAction(
            IResourceSetRepository resourceSetRepository,
            IResourceSetParameterValidator resourceSetParameterValidator)
        {
            _resourceSetRepository = resourceSetRepository;
            _resourceSetParameterValidator = resourceSetParameterValidator;
        }

        public async Task<string> Execute(AddResouceSetParameter addResourceSetParameter)
        {
            if (addResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(addResourceSetParameter));
            }

            var resourceSet = new ResourceSet
            {
                Id = Id.Create(),
                Name = addResourceSetParameter.Name,
                Uri = addResourceSetParameter.Uri,
                Type = addResourceSetParameter.Type,
                Scopes = addResourceSetParameter.Scopes,
                IconUri = addResourceSetParameter.IconUri
            };

            _resourceSetParameterValidator.CheckResourceSetParameter(resourceSet);
            if (!await _resourceSetRepository.Insert(resourceSet).ConfigureAwait(false))
            {
                throw new BaseUmaException(UmaErrorCodes.InternalError,
                    ErrorDescriptions.TheResourceSetCannotBeInserted);
            }

            return resourceSet.Id;
        }
    }
}
