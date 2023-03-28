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

namespace DotAuth.Controllers;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.PermissionController;
using DotAuth.Events;
using DotAuth.Filters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Uma;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the permission controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(UmaConstants.RouteValues.Permission)]
[ThrottleFilter]
[Authorize(Policy = "UmaProtection")]
public sealed class PermissionsController : BaseController
{
    private readonly ITicketStore _ticketStore;
    private readonly IResourceSetRepository _resourceSetRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PermissionsController> _logger;
    private readonly RequestPermissionHandler _requestPermission;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionsController"/> class.
    /// </summary>
    /// <param name="authenticationService">The default authentication service.</param>
    /// <param name="resourceSetRepository">The resource set repository.</param>
    /// <param name="ticketStore">The ticket store.</param>
    /// <param name="options">The options.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="logger">The logger</param>
    public PermissionsController(
        IAuthenticationService authenticationService,
        IResourceSetRepository resourceSetRepository,
        ITicketStore ticketStore,
        RuntimeSettings options,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        ILogger<PermissionsController> logger)
        : base(authenticationService)
    {
        _ticketStore = ticketStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _resourceSetRepository = resourceSetRepository;
        _requestPermission = new RequestPermissionHandler(tokenStore, resourceSetRepository, options, logger);
    }

    /// <summary>
    /// Gets the permission requests.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>All permission requests for the user.</returns>
    [HttpGet]
    public async Task<IActionResult> GetPermissionRequests(CancellationToken cancellationToken)
    {
        var owner = User.GetSubject()!;
        var tickets = await _ticketStore.GetAll(owner, cancellationToken).ConfigureAwait(false);

        return Ok(tickets);
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
        var (success, claims) = await _ticketStore.ApproveAccess(id, cancellationToken).ConfigureAwait(false);
        if (success)
        {
            await _eventPublisher.Publish(
                    new UmaRequestApproved(
                        Id.Create(),
                        id,
                        User.GetClientId()!,
                        User.GetSubject()!,
                        claims,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
        }

        return success
            ? RedirectToAction("GetPermissionRequests", "Permissions")
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
                string.Format(Strings.MissingParameter, "scopes"),
                HttpStatusCode.BadRequest);
        }

        var resourceSetOwner = await _resourceSetRepository
            .GetOwner(cancellationToken, permissionRequest.ResourceSetId)
            .ConfigureAwait(false);
        if (resourceSetOwner == null)
        {
            return BuildError(
                ErrorCodes.InvalidResourceSetId,
                string.Format(Strings.TheResourceSetDoesntExist, permissionRequest.ResourceSetId),
                HttpStatusCode.BadRequest);
        }

        var option = await _requestPermission.Execute(resourceSetOwner, cancellationToken, permissionRequest)
            .ConfigureAwait(false);
        return await CreateResultFromOption(option, resourceSetOwner, cancellationToken, permissionRequest).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds the permissions.
    /// </summary>
    /// <param name="permissionRequests">The post permissions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkRequestPermissions(
        [FromBody] PermissionRequest[]? permissionRequests,
        CancellationToken cancellationToken)
    {
        if (permissionRequests == null)
        {
            return BuildError(
                ErrorCodes.InvalidRequest,
                Strings.NoParameterInBodyRequest,
                HttpStatusCode.BadRequest);
        }

        var ids = permissionRequests.Where(x => !string.IsNullOrWhiteSpace(x.ResourceSetId))
            .Select(x => x.ResourceSetId!)
            .ToArray();
        var resourceSetOwner = await _resourceSetRepository.GetOwner(cancellationToken, ids).ConfigureAwait(false);
        if (resourceSetOwner == null)
        {
            return BuildError(ErrorCodes.InvalidResourceSetId, string.Join(" ", ids), HttpStatusCode.BadRequest);
        }

        var option = await _requestPermission.Execute(resourceSetOwner, cancellationToken, permissionRequests)
            .ConfigureAwait(false);
        return await CreateResultFromOption(option, resourceSetOwner, cancellationToken, permissionRequests)
            .ConfigureAwait(false);
    }

    private async Task<IActionResult> CreateResultFromOption(
        Option<Ticket>? option,
        string resourceSetOwner,
        CancellationToken cancellationToken,
        params PermissionRequest[] permissionRequests)
    {
        switch (option)
        {
            case Option<Ticket>.Error e:
                _logger.LogError("{error}", e.Details.Detail);
                return BadRequest(e.Details);
            case Option<Ticket>.Result r:
                var ticket = r.Item;
                var saved = await _ticketStore.Add(ticket, cancellationToken).ConfigureAwait(false);
                if (!saved)
                {
                    return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                }

                await _eventPublisher.Publish(
                        new UmaTicketCreated(
                            Id.Create(),
                            User.GetClientId()!,
                            ticket.Id,
                            resourceSetOwner,
                            User.GetSubject()!,
                            ticket.Requester,
                            DateTimeOffset.UtcNow,
                            permissionRequests))
                    .ConfigureAwait(false);
                var result = new TicketResponse { TicketId = ticket.Id };
                return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
            default: throw new Exception();
        }
    }

    private IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
    {
        var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
        _logger.LogError("{error}", message);
        return new BadRequestObjectResult(error) { StatusCode = (int)statusCode };
    }
}