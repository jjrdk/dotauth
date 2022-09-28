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

namespace SimpleAuth.WebSite.User;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleAuth.Properties;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;

internal sealed class UpdateUserClaimsOperation
{
    private readonly IResourceOwnerRepository _resourceOwnerRepository;
    private readonly ILogger _logger;

    public UpdateUserClaimsOperation(IResourceOwnerRepository resourceOwnerRepository, ILogger logger)
    {
        _resourceOwnerRepository = resourceOwnerRepository;
        _logger = logger;
    }

    public async Task<Option> Execute(string subject, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        var resourceOwner = await _resourceOwnerRepository.Get(subject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            _logger.LogError(Strings.TheRoDoesntExist);
            return new Option.Error(new ErrorDetails
            {
                Title = ErrorCodes.InternalError,
                Detail = Strings.TheRoDoesntExist,
                Status = HttpStatusCode.InternalServerError
            });
        }

        var claimsToBeRemoved = resourceOwner.Claims.Where(cl => claims.Any(c => c.Type == cl.Type))
            .ToArray();
        resourceOwner.Claims = resourceOwner.Claims.Remove(claimsToBeRemoved);

        resourceOwner.Claims =
            resourceOwner.Claims.Add(claims.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToArray());

        Claim? updatedClaim;
        if ((updatedClaim = resourceOwner.Claims.FirstOrDefault(
                c => c.Type == OpenIdClaimTypes.UpdatedAt))
            != null)
        {
            resourceOwner.Claims.Remove(updatedClaim);
        }

        resourceOwner.Claims = resourceOwner.Claims.Add(
            new Claim(OpenIdClaimTypes.UpdatedAt, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString()));
        return await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
    }
}