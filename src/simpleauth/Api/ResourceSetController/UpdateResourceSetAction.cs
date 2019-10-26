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
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using ResourceSet = SimpleAuth.Shared.DTOs.ResourceSet;

    internal class UpdateResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public UpdateResourceSetAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public async Task<bool> Execute(string owner, ResourceSet updateResourceSetParameter, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(updateResourceSetParameter.Id))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "id"));
            }

            var resourceSet = new ResourceSetModel
            {
                Id = updateResourceSetParameter.Id,
                Owner = owner,
                Name = updateResourceSetParameter.Name,
                Uri = updateResourceSetParameter.Uri,
                Type = updateResourceSetParameter.Type,
                Scopes = updateResourceSetParameter.Scopes ?? Array.Empty<string>(),
                IconUri = updateResourceSetParameter.IconUri,
                AuthorizationPolicyIds = updateResourceSetParameter.AuthorizationPolicies ?? Array.Empty<string>()
            };

            CheckResourceSetParameter(resourceSet);
            return await _resourceSetRepository.Update(resourceSet, cancellationToken).ConfigureAwait(false);
        }

        private void CheckResourceSetParameter(Shared.Models.ResourceSetModel resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            if (string.IsNullOrWhiteSpace(resourceSet.Name))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
            }

            if (resourceSet.Scopes == null || !resourceSet.Scopes.Any())
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));
            }

            if (!string.IsNullOrWhiteSpace(resourceSet.IconUri)
                && !Uri.IsWellFormedUriString(resourceSet.IconUri, UriKind.Absolute))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, resourceSet.IconUri));
            }

            if (!string.IsNullOrWhiteSpace(resourceSet.Uri)
                && !Uri.IsWellFormedUriString(resourceSet.Uri, UriKind.Absolute))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, resourceSet.Uri));
            }
        }
    }
}
