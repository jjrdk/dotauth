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

namespace SimpleAuth.WebSite.User.Actions
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal class GetUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public GetUserOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public Task<ResourceOwner> Execute(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (claimsPrincipal.Identity == null
                || !claimsPrincipal.Identity.IsAuthenticated
                || !(claimsPrincipal.Identity is ClaimsIdentity))
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }

            var subject = claimsPrincipal.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new SimpleAuthException(
                    ErrorCodes.UnhandledExceptionCode,
                    ErrorDescriptions.TheSubjectCannotBeRetrieved);
            }

            return _resourceOwnerRepository.Get(subject, cancellationToken);
        }
    }
}
