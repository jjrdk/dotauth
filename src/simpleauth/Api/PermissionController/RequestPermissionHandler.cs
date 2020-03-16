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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;

    internal class RequestPermissionHandler
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ITicketStore _ticketStore;
        private readonly RuntimeSettings _configurationService;

        public RequestPermissionHandler(
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
            params PermissionRequest[] addPermissionParameters)
        {
            var resourceOwner = await CheckAddPermissionParameter(addPermissionParameters, cancellationToken).ConfigureAwait(false);
            var builder = new StringBuilder();
            builder.Append(clientId);
            builder.Append(resourceOwner);
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                builder.Append(addPermissionParameter.ResourceSetId);
                foreach (var scope in addPermissionParameter.Scopes)
                {
                    builder.Append(scope);
                }
            }

            var ticket = new Ticket
            {
                Id = Id.Create(),
                ClientId = clientId,
                ResourceOwner = resourceOwner,
                Created = DateTimeOffset.UtcNow,
                Expires = DateTimeOffset.UtcNow.Add(_configurationService.TicketLifeTime),
                // TH : ONE TICKET FOR MULTIPLE PERMISSIONS.
                Lines = addPermissionParameters.Select(
                        addPermissionParameter => new TicketLine
                        {
                            Scopes = addPermissionParameter.Scopes,
                            ResourceSetId = addPermissionParameter.ResourceSetId
                        })
                    .ToArray()
            };

            if (!await _ticketStore.Add(ticket, cancellationToken).ConfigureAwait(false))
            {
                throw new SimpleAuthException(ErrorCodes.InternalError, ErrorDescriptions.TheTicketCannotBeInserted);
            }

            return ticket.Id;
        }

        private async Task<string> CheckAddPermissionParameter(
            PermissionRequest[] addPermissionParameters,
            CancellationToken cancellationToken)
        {
            var resourceSets = await _resourceSetRepository.Get(
                        cancellationToken,
                        addPermissionParameters.Select(p => p.ResourceSetId).ToArray())
                    .ConfigureAwait(false);

            if (resourceSets.Select(r => r.Owner).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count() != 1)
            {
                // All resource sets must belong to same owner
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.InvalidResourceSetRequest);
            }

            // 2. Check parameters & scope exist.
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequest,
                        string.Format(
                            ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddPermissionNames.ResourceSetId));
                }

                if (addPermissionParameter.Scopes == null || !addPermissionParameter.Scopes.Any())
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequest,
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

            return resourceSets.ElementAt(0).Owner;
        }
    }
}
