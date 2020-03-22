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
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

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
            string owner,
            CancellationToken cancellationToken,
            params PermissionRequest[] addPermissionParameters)
        {
            await CheckAddPermissionParameter(owner, addPermissionParameters, cancellationToken).ConfigureAwait(false);

            var ticket = new Ticket
            {
                Id = Id.Create(),
                Created = DateTimeOffset.UtcNow,
                Expires = DateTimeOffset.UtcNow.Add(_configurationService.TicketLifeTime),
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

        private async Task CheckAddPermissionParameter(
            string owner,
            PermissionRequest[] addPermissionParameters,
            CancellationToken cancellationToken)
        {
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

                var resourceSet = await _resourceSetRepository.Get(
                    owner,
                    addPermissionParameter.ResourceSetId,
                    cancellationToken).ConfigureAwait(false);

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
