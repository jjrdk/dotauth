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
    using Errors;
    using Exceptions;
    using Parameters;
    using Repositories;
    using Shared;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class AddPermissionAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ITicketStore _ticketStore;
        private readonly UmaConfigurationOptions _configurationService;

        public AddPermissionAction(
            IResourceSetRepository resourceSetRepository,
            ITicketStore ticketStore,
            UmaConfigurationOptions configurationService)
        {
            _resourceSetRepository = resourceSetRepository;
            _ticketStore = ticketStore;
            _configurationService = configurationService;
        }

        public async Task<string> Execute(string clientId, AddPermissionParameter addPermissionParameter)
        {
            var result = await Execute(clientId, new[] { addPermissionParameter }).ConfigureAwait(false);
            return result;
        }

        public async Task<string> Execute(string clientId, IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(clientId);
            }

            if (addPermissionParameters == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameters));
            }

            await CheckAddPermissionParameter(addPermissionParameters).ConfigureAwait(false);
            var ticketLifetimeInSeconds = _configurationService.TicketLifeTime;
            var ticket = new Ticket
            {
                Id = Id.Create(),
                ClientId = clientId,
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = (int)ticketLifetimeInSeconds.TotalSeconds,
                ExpirationDateTime = DateTime.UtcNow.Add(ticketLifetimeInSeconds)
            };
            // TH : ONE TICKET FOR MULTIPLE PERMISSIONS.
            var ticketLines = addPermissionParameters.Select(addPermissionParameter => new TicketLine
            {
                Id = Id.Create(),
                Scopes = addPermissionParameter.Scopes,
                ResourceSetId = addPermissionParameter.ResourceSetId
            })
                .ToList();

            ticket.Lines = ticketLines;
            if (!await _ticketStore.AddAsync(ticket).ConfigureAwait(false))
            {
                throw new SimpleAuthException(ErrorCodes.InternalError, ErrorDescriptions.TheTicketCannotBeInserted);
            }

            return ticket.Id;
        }

        private async Task CheckAddPermissionParameter(IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            // 1. Get resource sets.

            IEnumerable<ResourceSet> resourceSets = null;
            try
            {
                resourceSets = await _resourceSetRepository.Get(addPermissionParameters.Select(p => p.ResourceSetId));
            }
            catch (Exception ex)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InternalError,
                    ErrorDescriptions.TheResourceSetsCannotBeRetrieved,
                    ex);
            }

            // 2. Check parameters & scope exist.
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddPermissionNames.ResourceSetId));
                }

                if (addPermissionParameter.Scopes == null ||
                    !addPermissionParameter.Scopes.Any())
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified,
                            UmaConstants.AddPermissionNames.Scopes));
                }

                var resourceSet = resourceSets.FirstOrDefault(r => addPermissionParameter.ResourceSetId == r.Id);
                if (resourceSet == null)
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidResourceSetId,
                        string.Format(ErrorDescriptions.TheResourceSetDoesntExist,
                            addPermissionParameter.ResourceSetId));
                }

                if (resourceSet.Scopes == null ||
                    addPermissionParameter.Scopes.Any(s => !resourceSet.Scopes.Contains(s)))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidScope, ErrorDescriptions.TheScopeAreNotValid);
                }
            }
        }
    }
}
