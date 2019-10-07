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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;

    internal class AddPermissionAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ITicketStore _ticketStore;
        private readonly RuntimeSettings _configurationService;

        public AddPermissionAction(
            IResourceSetRepository resourceSetRepository,
            ITicketStore ticketStore,
            RuntimeSettings configurationService)
        {
            _resourceSetRepository = resourceSetRepository;
            _ticketStore = ticketStore;
            _configurationService = configurationService;
        }

        public async Task<string> Execute(
            string clientId,
            CancellationToken cancellationToken,
            params PostPermission[] addPermissionParameters)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(clientId);
            }

            if (addPermissionParameters == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameters));
            }

            await CheckAddPermissionParameter(addPermissionParameters, cancellationToken).ConfigureAwait(false);
            var ticketLifetimeInSeconds = _configurationService.TicketLifeTime;
            var ticket = new Ticket
            {
                Id = Id.Create(),
                ClientId = clientId,
                CreateDateTime = DateTimeOffset.UtcNow,
                ExpiresIn = (int)ticketLifetimeInSeconds.TotalSeconds,
                ExpirationDateTime = DateTimeOffset.UtcNow.Add(ticketLifetimeInSeconds)
            };
            // TH : ONE TICKET FOR MULTIPLE PERMISSIONS.
            var ticketLines = addPermissionParameters.Select(
                    addPermissionParameter => new TicketLine
                    {
                        Id = Id.Create(),
                        Scopes = addPermissionParameter.Scopes,
                        ResourceSetId = addPermissionParameter.ResourceSetId
                    })
                .ToArray();

            ticket.Lines = ticketLines;
            if (!await _ticketStore.Add(ticket, cancellationToken).ConfigureAwait(false))
            {
                throw new SimpleAuthException(ErrorCodes.InternalError, ErrorDescriptions.TheTicketCannotBeInserted);
            }

            return ticket.Id;
        }

        private async Task CheckAddPermissionParameter(
            PostPermission[] addPermissionParameters,
            CancellationToken cancellationToken)
        {
            var resourceSets = await _resourceSetRepository.Get(
                        cancellationToken,
                        addPermissionParameters.Select(p => p.ResourceSetId).ToArray())
                    .ConfigureAwait(false);

            // 2. Check parameters & scope exist.
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(
                            ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddPermissionNames.ResourceSetId));
                }

                if (addPermissionParameter.Scopes == null || !addPermissionParameter.Scopes.Any())
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(
                            ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddPermissionNames.Scopes));
                }

                var resourceSet = resourceSets.FirstOrDefault(r => addPermissionParameter.ResourceSetId == r.Id);
                if (resourceSet == null)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidResourceSetId,
                        string.Format(
                            ErrorDescriptions.TheResourceSetDoesntExist,
                            addPermissionParameter.ResourceSetId));
                }

                if (resourceSet.Scopes == null
                    || addPermissionParameter.Scopes.Any(s => !resourceSet.Scopes.Contains(s)))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope, ErrorDescriptions.TheScopeAreNotValid);
                }
            }
        }
    }
}
