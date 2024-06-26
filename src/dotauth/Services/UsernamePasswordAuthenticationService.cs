﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Properties;
using DotAuth.Shared.Events.Logging;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class UsernamePasswordAuthenticationService : IAuthenticateResourceOwnerService
{
    private readonly IResourceOwnerStore _resourceOwnerRepository;
    private readonly ILogger<IAuthenticateResourceOwnerService> _logger;
    private readonly IEventPublisher _eventPublisher;

    public UsernamePasswordAuthenticationService(
        IResourceOwnerStore resourceOwnerRepository,
        ILogger<IAuthenticateResourceOwnerService> logger,
        IEventPublisher eventPublisher)
    {
        _resourceOwnerRepository = resourceOwnerRepository;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    public string Amr
    {
        get { return "pwd"; }
    }

    public async Task<ResourceOwner?> AuthenticateResourceOwner(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var resourceOwner =
            await _resourceOwnerRepository.Get(login, password, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            _logger.LogError(Strings.LogCouldNotAuthenticate, login);
        }
        else
        {
            await _eventPublisher.Publish(
                    new ResourceOwnerAuthenticated(Id.Create(), resourceOwner.Subject, DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            _logger.LogInformation(Strings.LogAuthenticated, resourceOwner.Subject);
        }

        return resourceOwner;
    }
}