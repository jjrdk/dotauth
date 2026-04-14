namespace DotAuth.Endpoints;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using Microsoft.AspNetCore.Http;

internal static class ScopesEndpointHandlers
{
    private const string GetAllScopesView = "/Views/Scopes/GetAll.cshtml";
    private const string GetScopeView = "/Views/Scopes/Get.cshtml";

    internal static async Task<IResult> Search(
        HttpContext httpContext,
        SearchScopesRequest? request,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Results.BadRequest();
        }

        var result = await scopeRepository.Search(request, cancellationToken).ConfigureAwait(false);
        return Results.Json(result);
    }

    internal static async Task<IResult> GetAll(
        HttpContext httpContext,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        var result = await scopeRepository.GetAll(cancellationToken).ConfigureAwait(false);
        return UiEndpointHelpers.ViewOrJson(httpContext, GetAllScopesView, result);
    }

    internal static async Task<IResult> Get(
        HttpContext httpContext,
        string id,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        var scope = await scopeRepository.Get(id, cancellationToken).ConfigureAwait(false);
        return scope == null ? Results.BadRequest() : UiEndpointHelpers.ViewOrJson(httpContext, GetScopeView, scope);
    }

    internal static async Task<IResult> Delete(
        string id,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var scope = await scopeRepository.Get(id, cancellationToken).ConfigureAwait(false);
        if (scope == null)
        {
            return Results.BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = string.Format(Strings.TheScopeDoesntExist, id),
                    Status = System.Net.HttpStatusCode.BadRequest
                });
        }

        var deleted = await scopeRepository.Delete(scope, CancellationToken.None).ConfigureAwait(false);
        return deleted
            ? Results.StatusCode(StatusCodes.Status204NoContent)
            : Results.BadRequest(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = string.Format(Strings.TheScopeDoesntExist, id),
                    Status = System.Net.HttpStatusCode.BadRequest
                });
    }

    internal static async Task<IResult> Add(
        Scope? request,
        IScopeRepository scopeRepository)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var ok = await scopeRepository.Insert(request, CancellationToken.None).ConfigureAwait(false);
        return ok ? Results.StatusCode(StatusCodes.Status204NoContent) : Results.StatusCode(StatusCodes.Status500InternalServerError);
    }

    internal static async Task<IResult> UpdateByName(
        HttpContext httpContext,
        string name,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        var scope = await EndpointHandlerHelpers.BindFromFormAsync<Scope>(httpContext.Request).ConfigureAwait(false);
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        scope = scope with { Name = name };
        var updated = await scopeRepository.Update(scope, cancellationToken).ConfigureAwait(false);
        return updated ? Results.Redirect($"/{CoreConstants.EndPoints.Scopes}") : Results.StatusCode(StatusCodes.Status500InternalServerError);
    }

    internal static async Task<IResult> Update(
        Scope? request,
        IScopeRepository scopeRepository,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var ok = await scopeRepository.Update(request, cancellationToken).ConfigureAwait(false);
        return ok ? Results.StatusCode(StatusCodes.Status204NoContent) : Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
}

