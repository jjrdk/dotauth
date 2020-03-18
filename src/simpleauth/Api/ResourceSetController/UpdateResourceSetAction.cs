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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Models;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using ResourceSet = SimpleAuth.Shared.Models.ResourceSet;

    internal class UpdateResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public UpdateResourceSetAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(ResourceSet resourceSet, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(resourceSet.Id))
            {
                return false;
            }

            CheckResourceSetParameter(resourceSet);
            return await _resourceSetRepository.Update(resourceSet, cancellationToken).ConfigureAwait(false);
        }

        private void CheckResourceSetParameter(Shared.Models.ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            if (string.IsNullOrWhiteSpace(resourceSet.Name))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
            }

            if (resourceSet.Scopes == null || !resourceSet.Scopes.Any())
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));
            }
        }
    }
}
