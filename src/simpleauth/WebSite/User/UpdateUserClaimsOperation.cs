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

namespace SimpleAuth.WebSite.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;

    internal class UpdateUserClaimsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserClaimsOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheRoDoesntExist);
            }

            var claimsToBeRemoved = resourceOwner.Claims.Where(cl => claims.Any(c => c.Type == cl.Type))
                .ToArray();
            resourceOwner.Claims = resourceOwner.Claims.Remove(claimsToBeRemoved);

            resourceOwner.Claims =
                resourceOwner.Claims.Add(claims.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToArray());

            Claim updatedClaim;
            if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(
                     c => c.Type == OpenIdClaimTypes.UpdatedAt))
                 != null))
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            resourceOwner.Claims = resourceOwner.Claims.Add(
                     new Claim(OpenIdClaimTypes.UpdatedAt, DateTime.UtcNow.ToString()));
            return await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        }
    }
}
