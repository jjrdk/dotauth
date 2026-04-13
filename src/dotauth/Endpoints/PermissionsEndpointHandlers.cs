namespace DotAuth.Endpoints;

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.PermissionController;
using DotAuth.Events;
using DotAuth.Extensions;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class PermissionsEndpointHandlers
{
    internal static async Task<IResult> GetPermissionRequests(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        ITicketStore ticketStore,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var owner = httpContext.User.GetSubject()!;
        var tickets = await ticketStore.GetAll(owner, cancellationToken).ConfigureAwait(false);
        return Results.Ok(tickets);
    }

    internal static async Task<IResult> ApprovePermissionRequest(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        ITicketStore ticketStore,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var (success, claims) = await ticketStore.ApproveAccess(id, cancellationToken).ConfigureAwait(false);
        if (success)
        {
            await eventPublisher.Publish(
                    new UmaRequestApproved(
                        Id.Create(),
                        id,
                        httpContext.User.GetClientId(),
                        httpContext.User.GetSubject()!,
                        claims,
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
        }

        return success
            ? Results.Redirect($"/{UmaConstants.RouteValues.Permission}")
            : Results.Json(
                new ErrorViewModel
                {
                    Title = Strings.UpdateFailed,
                    Message = string.Format(Strings.CouldNotUpdateApproval, id),
                    Code = (int)HttpStatusCode.BadRequest
                },
                statusCode: StatusCodes.Status400BadRequest);
    }

    internal static async Task<IResult> RequestPermission(
        HttpContext httpContext,
        PermissionRequest permissionRequest,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        RuntimeSettings options,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        ITicketStore ticketStore,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.PermissionsController");
        if (string.IsNullOrWhiteSpace(permissionRequest.ResourceSetId))
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, Strings.ResourceSetIdParameterNeedsToBeSpecified, HttpStatusCode.BadRequest);
        }

        if (permissionRequest.Scopes == null)
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, string.Format(Strings.MissingParameter, "scopes"), HttpStatusCode.BadRequest);
        }

        var resourceSetOwner = await resourceSetRepository.GetOwner(cancellationToken, permissionRequest.ResourceSetId).ConfigureAwait(false);
        if (resourceSetOwner == null)
        {
            return BuildError(logger, ErrorCodes.InvalidResourceSetId, string.Format(Strings.TheResourceSetDoesntExist, permissionRequest.ResourceSetId), HttpStatusCode.BadRequest);
        }

        var requestPermission = new RequestPermissionHandler(tokenStore, resourceSetRepository, options, logger);
        var option = await requestPermission.Execute(resourceSetOwner, cancellationToken, permissionRequest).ConfigureAwait(false);
        return await CreateResultFromOption(httpContext, option, resourceSetOwner, ticketStore, eventPublisher, logger, cancellationToken, permissionRequest).ConfigureAwait(false);
    }

    internal static async Task<IResult> BulkRequestPermissions(
        HttpContext httpContext,
        PermissionRequest[]? permissionRequests,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        RuntimeSettings options,
        IEventPublisher eventPublisher,
        ITokenStore tokenStore,
        ITicketStore ticketStore,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = loggerFactory.CreateLogger("DotAuth.Controllers.PermissionsController");
        if (permissionRequests == null)
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, Strings.NoParameterInBodyRequest, HttpStatusCode.BadRequest);
        }

        var ids = permissionRequests.Where(x => !string.IsNullOrWhiteSpace(x.ResourceSetId)).Select(x => x.ResourceSetId!).ToArray();
        var resourceSetOwner = await resourceSetRepository.GetOwner(cancellationToken, ids).ConfigureAwait(false);
        if (resourceSetOwner == null)
        {
            return BuildError(logger, ErrorCodes.InvalidResourceSetId, string.Join(" ", ids), HttpStatusCode.BadRequest);
        }

        var requestPermission = new RequestPermissionHandler(tokenStore, resourceSetRepository, options, logger);
        var option = await requestPermission.Execute(resourceSetOwner, cancellationToken, permissionRequests).ConfigureAwait(false);
        return await CreateResultFromOption(httpContext, option, resourceSetOwner, ticketStore, eventPublisher, logger, cancellationToken, permissionRequests).ConfigureAwait(false);
    }

    private static async Task<IResult> CreateResultFromOption(
        HttpContext httpContext,
        Option<Ticket>? option,
        string resourceSetOwner,
        ITicketStore ticketStore,
        IEventPublisher eventPublisher,
        ILogger logger,
        CancellationToken cancellationToken,
        params PermissionRequest[] permissionRequests)
    {
        switch (option)
        {
            case Option<Ticket>.Error e:
                logger.LogError("{Error}", e.Details.Detail);
                return Results.BadRequest(e.Details);
            case Option<Ticket>.Result r:
                var ticket = r.Item;
                var saved = await ticketStore.Add(ticket, cancellationToken).ConfigureAwait(false);
                if (!saved)
                {
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                await eventPublisher.Publish(
                        new UmaTicketCreated(
                            Id.Create(),
                            httpContext.User.GetClientId(),
                            ticket.Id,
                            resourceSetOwner,
                            httpContext.User.GetSubject()!,
                            ticket.Requester,
                            DateTimeOffset.UtcNow,
                            permissionRequests))
                    .ConfigureAwait(false);
                return Results.Json(new TicketResponse { TicketId = ticket.Id }, statusCode: StatusCodes.Status201Created);
            default:
                throw new Exception();
        }
    }

    private static IResult BuildError(ILogger logger, string code, string message, HttpStatusCode statusCode)
    {
        var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
        logger.LogError("{Error}", message);
        return Results.Json(error, statusCode: (int)statusCode);
    }
}

