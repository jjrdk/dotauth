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

namespace DotAuth.WebSite.User;

using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class GetUserOperation
{
    private readonly IResourceOwnerRepository _resourceOwnerRepository;
    private readonly ILogger _logger;

    public GetUserOperation(IResourceOwnerRepository resourceOwnerRepository, ILogger logger)
    {
        _resourceOwnerRepository = resourceOwnerRepository;
        _logger = logger;
    }

    public async Task<Option<ResourceOwner>> Execute(
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
        if (claimsIdentity?.IsAuthenticated != true)
        {
            _logger.LogError(Strings.TheUserNeedsToBeAuthenticated);
            return new Option<ResourceOwner>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = Strings.TheUserNeedsToBeAuthenticated,
                    Status = HttpStatusCode.InternalServerError
                });
        }

        var subject = claimsPrincipal.GetSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            _logger.LogError("{Error}", Strings.TheSubjectCannotBeRetrieved);
            return new Option<ResourceOwner>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.UnhandledExceptionCode,
                    Detail = Strings.TheSubjectCannotBeRetrieved,
                    Status = HttpStatusCode.InternalServerError
                });
        }

        var ro = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
        if (ro is not null)
        {
            return new Option<ResourceOwner>.Result(ro);
        }

        _logger.LogError(Strings.TheSubjectCannotBeRetrieved);
        return new Option<ResourceOwner>.Error(
            new ErrorDetails
            {
                Title = ErrorCodes.UnhandledExceptionCode,
                Detail = Strings.TheRoDoesntExist,
                Status = HttpStatusCode.InternalServerError
            });
    }
}
