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

namespace SimpleAuth.Api.PermissionController
{
    using Shared;
    using Shared.Models;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    internal class RequestPermissionHandler
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly RuntimeSettings _settings;
        private readonly ILogger _logger;

        public RequestPermissionHandler(
            IResourceSetRepository resourceSetRepository,
            RuntimeSettings settings,
            ILogger logger)
        {
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
            var handler = new JwtSecurityTokenHandler();
            var claims = addPermissionParameters.Select(x => x.IdToken)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(x => handler.ReadJwtToken(x))
                .SelectMany(x => x.Claims)
                .Where(claim => OpenIdClaimTypes.All.Contains(claim.Type))
                .Select(x => new ClaimData { Type = x.Type, Value = x.Value })
                .ToArray();

            var ticket = new Ticket
            {
                Id = Id.Create(),
                ResourceOwner = owner,
                Requester = claims,
                Created = DateTimeOffset.UtcNow,
                Expires = DateTimeOffset.UtcNow.Add(_settings.TicketLifeTime),
                Lines = addPermissionParameters.Where(x => x.Scopes != null && x.ResourceSetId != null)
                    .Select(
                        addPermissionParameter => new TicketLine
                        {
                            Scopes = addPermissionParameter.Scopes!,
                            ResourceSetId = addPermissionParameter.ResourceSetId!
                        })
                    .ToArray()
            };
            //if (!await _ticketStore.Add(ticket, cancellationToken).ConfigureAwait(false))
            //{
            //    return new Option<(string, ClaimData[])>.Error(
            //        new ErrorDetails
            //        {
            //            Title = ErrorCodes.InternalError,
            //            Detail = Strings.TheTicketCannotBeInserted,
            //            Status = HttpStatusCode.BadRequest
            //        },
            //        ticket.Id);
            //}

            return new Option<Ticket>.Result(ticket);
        }

        private async Task<Option> CheckAddPermissionParameter(
            string owner,
            PermissionRequest[] addPermissionParameters,
            CancellationToken cancellationToken)
        {
            // 2. Check parameters & scope exist.
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
                {
                    var message = string.Format(
                        Strings.MissingParameter,
                        UmaConstants.AddPermissionNames.ResourceSetId);
                    _logger.LogError(message);
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
                    _logger.LogError(message);
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
                    _logger.LogError(message);
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
}
