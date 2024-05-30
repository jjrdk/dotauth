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

namespace DotAuth.Api.PermissionController;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using Microsoft.Extensions.Logging;

internal sealed class RequestPermissionHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly IResourceSetRepository _resourceSetRepository;
    private readonly RuntimeSettings _settings;
    private readonly ILogger _logger;

    public RequestPermissionHandler(
        ITokenStore tokenStore,
        IResourceSetRepository resourceSetRepository,
        RuntimeSettings settings,
        ILogger logger)
    {
        _tokenStore = tokenStore;
        _resourceSetRepository = resourceSetRepository;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Option<Ticket>> Execute(
        string owner,
        CancellationToken cancellationToken,
        params PermissionRequest[] addPermissionParameters)
    {
        var result = await CheckAddPermissionParameter(owner, addPermissionParameters, cancellationToken).ConfigureAwait(false);
        if (result is Option.Error e)
        {
            return new Option<Ticket>.Error(e.Details);
        }
        var token = addPermissionParameters.Select(x => x.IdToken).First();
        var grantedToken = token == null ? null : await _tokenStore.GetAccessToken(token, cancellationToken).ConfigureAwait(false);
        var payload = grantedToken?.IdTokenPayLoad;
        if (payload == null && token != null)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            payload = jwt.Payload;
        }

        var claims = payload?.Claims
            .Where(claim => OpenIdClaimTypes.All.Contains(claim.Type))
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Value))
            .Select(x => new ClaimData { Type = x.Type, Value = x.Value })
            .ToArray() ?? [];

        var lines = addPermissionParameters.Where(x => x is { Scopes: { }, ResourceSetId: { } })
            .Select(
                addPermissionParameter => new TicketLine
                {
                    Scopes = addPermissionParameter.Scopes!,
                    ResourceSetId = addPermissionParameter.ResourceSetId!
                })
            .ToArray();

        var ticket = new Ticket
        {
            Id = Id.Create(),
            ResourceOwner = owner,
            Requester = claims,
            Created = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.Add(_settings.TicketLifeTime),
            Lines = lines
        };

        return new Option<Ticket>.Result(ticket);
    }

    private async Task<Option> CheckAddPermissionParameter(
        string owner,
        PermissionRequest[] addPermissionParameters,
        CancellationToken cancellationToken)
    {
        var idTokens = addPermissionParameters.Select(x => x.IdToken)
            .Distinct()
            .Count();
        if (idTokens > 1)
        {
            return new Option.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.AmbiguousRequestor,
                    Detail = Strings.AmbiguousIdentity,
                    Status = HttpStatusCode.BadRequest
                });
        }
        // 2. Check parameters & scope exist.
        foreach (var addPermissionParameter in addPermissionParameters)
        {
            if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
            {
                var message = string.Format(
                    Strings.MissingParameter,
                    UmaConstants.AddPermissionNames.ResourceSetId);
                _logger.LogError(Strings.MissingParameter, UmaConstants.AddPermissionNames.ResourceSetId);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            if (addPermissionParameter.Scopes == null || !addPermissionParameter.Scopes.Any())
            {
                var message = string.Format(Strings.MissingParameter, UmaConstants.AddPermissionNames.Scopes);
                _logger.LogError(Strings.MissingParameter, UmaConstants.AddPermissionNames.Scopes);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var resourceSet = await _resourceSetRepository.Get(
                    owner,
                    addPermissionParameter.ResourceSetId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (resourceSet == null)
            {
                var message = string.Format(
                    Strings.TheResourceSetDoesntExist,
                    addPermissionParameter.ResourceSetId);
                _logger.LogError(Strings.TheResourceSetDoesntExist, addPermissionParameter.ResourceSetId);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidResourceSetId,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            }

            if (addPermissionParameter.Scopes.Any(s => !resourceSet.Scopes.Contains(s)))
            {
                var message = Strings.TheScopeAreNotValid;
                _logger.LogError(message);
                return new Option.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidScope,
                        Detail = message,
                        Status = HttpStatusCode.BadRequest
                    });
            }
        }

        return new Option.Success();
    }
}