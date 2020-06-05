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

namespace SimpleAuth.Controllers
{
    using System;
    using Api.PermissionController;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Responses;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Events;
    using SimpleAuth.Filters;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Events.Uma;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.ViewModels;

    /// <summary>
    /// Defines the permission controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(UmaConstants.RouteValues.Permission)]
    [ThrottleFilter]
    [Authorize(Policy = "UmaProtection")]
    public class PermissionsController : ControllerBase
    {
        private readonly ITicketStore _ticketStore;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly RequestPermissionHandler _requestPermission;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsController"/> class.
        /// </summary>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        /// <param name="ticketStore">The ticket store.</param>
        /// <param name="options">The options.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        public PermissionsController(
            IResourceSetRepository resourceSetRepository,
            ITicketStore ticketStore,
            RuntimeSettings options,
            IEventPublisher eventPublisher)
        {
            _ticketStore = ticketStore;
            _eventPublisher = eventPublisher;
            _resourceSetRepository = resourceSetRepository;
            _requestPermission = new RequestPermissionHandler(resourceSetRepository, ticketStore, options);
        }

        /// <summary>
        /// Gets the permission requests.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>All permission requests for the user.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPermissionRequests(CancellationToken cancellationToken)
        {
            var owner = User.GetSubject();
            var tickets = await _ticketStore.GetAll(owner, cancellationToken).ConfigureAwait(false);

            return Ok(tickets.Where(x => x.IsAuthorizedByRo == false).ToArray());
        }

        /// <summary>
        /// Approves the permission request by the resource owner.
        /// </summary>
        /// <param name="id">The ticket id to approve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>If successful, redirects to the open permission request, otherwise returns an error.</returns>
        [HttpPost]
        [Route("{id}/approve")]
        public async Task<IActionResult> ApprovePermissionRequest(string id, CancellationToken cancellationToken)
        {
            var (success, claims) = await _ticketStore.ApproveAccess(id, cancellationToken);
            if (success)
            {
                await _eventPublisher.Publish(
                        new UmaRequestApproved(
                            Id.Create(),
                            id,
                            User.GetClientId(),
                            User.GetSubject(),
                            claims,
                            DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
            }

            return success
                ? (IActionResult)RedirectToAction("GetPermissionRequests", "Permissions")
                : BadRequest(
                    new ErrorViewModel
                    {
                        Title = Strings.UpdateFailed,
                        Message = string.Format(Strings.CouldNotUpdateApproval, id),
                        Code = (int)HttpStatusCode.BadRequest
                    });
        }

        /// <summary>
        /// Adds the permission.
        /// </summary>
        /// <param name="permissionRequest">The post permission.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RequestPermission(
            [FromBody] PermissionRequest permissionRequest,
            CancellationToken cancellationToken)
        {
            if (permissionRequest == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    Strings.NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(permissionRequest.ResourceSetId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    Strings.ResourceSetIdParameterNeedsToBeSpecified,
                    HttpStatusCode.BadRequest);
            }

            if (permissionRequest.Scopes == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    string.Format(Strings.TheParameterNeedsToBeSpecified, "scopes"),
                    HttpStatusCode.BadRequest);
            }

            var subject = User.GetSubject();
            var resourceSetOwner = await _resourceSetRepository.GetOwner(cancellationToken, permissionRequest.ResourceSetId);
            if (resourceSetOwner == null)
            {
                return BuildError(
                    ErrorCodes.InvalidResourceSetId,
                    string.Format(Strings.TheResourceSetDoesntExist, permissionRequest.ResourceSetId),
                    HttpStatusCode.BadRequest);
            }

            var (ticketId, requesterClaims) = await _requestPermission
                .Execute(resourceSetOwner, cancellationToken, permissionRequest)
                .ConfigureAwait(false);
            await _eventPublisher.Publish(
                    new UmaTicketCreated(
                        Id.Create(),
                        User.GetClientId(),
                        ticketId,
                        resourceSetOwner,
                        subject,
                        requesterClaims,
                        DateTimeOffset.UtcNow,
                        permissionRequest))
                .ConfigureAwait(false);
            var result = new TicketResponse { TicketId = ticketId };
            return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
        }

        /// <summary>
        /// Adds the permissions.
        /// </summary>
        /// <param name="permissionRequests">The post permissions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkRequestPermissions(
            [FromBody] PermissionRequest[] permissionRequests,
            CancellationToken cancellationToken)
        {
            if (permissionRequests == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    Strings.NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }
            
            var ids = permissionRequests.Select(x => x.ResourceSetId).ToArray();
            var resourceSetOwner = await _resourceSetRepository.GetOwner(cancellationToken, ids);
            if (resourceSetOwner == null)
            {
                return BuildError(
                    ErrorCodes.InvalidResourceSetId,
                    string.Join(" ", ids),
                    HttpStatusCode.BadRequest);
            }

            var subject = User.GetSubject();
            var (ticketId, requesterClaims) = await _requestPermission
                .Execute(resourceSetOwner, cancellationToken, permissionRequests)
                .ConfigureAwait(false);
            await _eventPublisher.Publish(
                    new UmaTicketCreated(
                        Id.Create(),
                        User.GetClientId(),
                        ticketId,
                        resourceSetOwner,
                        subject,
                        requesterClaims,
                        DateTimeOffset.UtcNow,
                        permissionRequests))
                .ConfigureAwait(false);
            var result = new TicketResponse { TicketId = ticketId };
            return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
        }

        private static IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
            return new BadRequestObjectResult(error) { StatusCode = (int)statusCode };
        }
    }
}
