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

namespace SimpleAuth.Api.ResourceSetController
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Repositories;

    internal class DeleteResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public DeleteResourceSetAction(
            IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(string resourceSetId)
        {
            if (string.IsNullOrWhiteSpace(resourceSetId))
            {
                throw new ArgumentNullException(nameof(resourceSetId));
            }

            var result = await _resourceSetRepository.Get(resourceSetId).ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            if (!await _resourceSetRepository.Delete(resourceSetId).ConfigureAwait(false))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
            }

            return true;
        }
    }
}
