namespace DotAuth.Endpoints;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.ResourceSetController;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class ResourceSetEndpointHandlers
{
    private const string GetResourceSetsView = "/Views/ResourceSet/GetResourceSets.cshtml";
    private const string GetResourceSetPolicyView = "/Views/ResourceSet/GetResourceSetPolicy.cshtml";
    private const string SetResourceSetPolicyView = "/Views/ResourceSet/SetResourceSetPolicy.cshtml";

    internal static async Task<IResult> SearchResourceSets(
        HttpContext httpContext,
        SearchResourceSet? searchResourceSet,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (searchResourceSet?.Terms.Length == 0)
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.NoParameterInBodyRequest, HttpStatusCode.BadRequest);
        }

        var result = await resourceSetRepository.Search(httpContext.User.Claims.ToArray(), searchResourceSet!, cancellationToken).ConfigureAwait(false);
        return Results.Ok(
            new PagedResult<ResourceSetDescription>
            {
                Content = result.Content,
                StartIndex = result.StartIndex,
                TotalResults = result.TotalResults
            });
    }

    internal static async Task<IResult> GetResourceSets(
        HttpContext httpContext,
        string? ui,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var owner = httpContext.User.GetSubject();
        if (string.IsNullOrWhiteSpace(owner))
        {
            return Results.BadRequest();
        }

        var resourceSets = await resourceSetRepository.GetAll(owner, cancellationToken).ConfigureAwait(false);
        if (UiEndpointHelpers.WantsHtml(httpContext.Request))
        {
            return UiEndpointHelpers.ViewOrJson(
                httpContext,
                GetResourceSetsView,
                resourceSets.Select(ResourceSetViewModel.FromResourceSet).ToArray());
        }

        var value = ui == "1"
            ? (object)resourceSets.Select(ResourceSetViewModel.FromResourceSet).ToArray()
            : resourceSets.Select(x => x.Id).ToArray();

        return Results.Ok(value);
    }

    internal static async Task<IResult> GetResourceSet(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var subject = httpContext.User.GetSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheSubjectCannotBeRetrieved, HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheIdentifierMustBeSpecified, HttpStatusCode.BadRequest);
        }

        var result = await resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
        return result == null ? Results.NoContent() : Results.Ok(result);
    }

    internal static async Task<IResult> GetResourceSetPolicy(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var subject = httpContext.User.GetSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheSubjectCannotBeRetrieved, HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheIdentifierMustBeSpecified, HttpStatusCode.BadRequest);
        }

        var result = await resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return Results.BadRequest();
        }

        var response = new EditPolicyResponse { Id = id, Rules = result.AuthorizationPolicies.Select(ToViewModel).ToArray() };
        return UiEndpointHelpers.ViewOrJson(httpContext, GetResourceSetPolicyView, response);
    }

    internal static async Task<IResult> SetResourceSetPolicyFromViewModel(
        HttpContext httpContext,
        EditPolicyResponse? viewModel,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (viewModel == null)
        {
            return Results.BadRequest(
                new ErrorDetails
                {
                    Detail = Strings.InputMissing,
                    Title = Strings.InputMissing,
                    Status = HttpStatusCode.BadRequest
                });
        }

        var result = await SetResourceSetPolicy(httpContext, viewModel.Id, viewModel.Rules.Select(ToModel).ToArray(), resourceSetRepository, cancellationToken).ConfigureAwait(false);
        return result is not null ? result : UiEndpointHelpers.ViewOrJson(httpContext, SetResourceSetPolicyView, new object());
    }

    internal static async Task<IResult> SetResourceSetPolicy(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var rules = await ReadPolicyRulesAsync(httpContext.Request, cancellationToken).ConfigureAwait(false);
        if (rules == null)
        {
            return Results.BadRequest();
        }

        var result = await SetResourceSetPolicy(httpContext, id, rules, resourceSetRepository, cancellationToken).ConfigureAwait(false);
        return result ?? UiEndpointHelpers.ViewOrJson(httpContext, SetResourceSetPolicyView, new object());
    }

    internal static async Task<IResult> AddResourceSet(
        HttpContext httpContext,
        ResourceSet resourceSet,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var subject = httpContext.User.GetSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheSubjectCannotBeRetrieved, HttpStatusCode.BadRequest);
        }

        if (resourceSet.IconUri != null && !resourceSet.IconUri.IsAbsoluteUri)
        {
            return BuildError(ErrorCodes.InvalidUri, Strings.TheUrlIsNotWellFormed, HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(resourceSet.Name))
        {
            return BuildError(ErrorCodes.InvalidRequest, string.Format(Strings.MissingParameter, "name"), HttpStatusCode.BadRequest);
        }

        if (resourceSet.Scopes.Length == 0)
        {
            resourceSet = resourceSet with
            {
                Scopes = resourceSet.AuthorizationPolicies.Length == 0
                    ? ["read"]
                    : resourceSet.AuthorizationPolicies.SelectMany(p => p.Scopes).ToArray()
            };
        }

        resourceSet = resourceSet.AuthorizationPolicies.Length == 0
            ? resourceSet with
            {
                Id = Id.Create(),
                AuthorizationPolicies = [new PolicyRule { IsResourceOwnerConsentNeeded = true, Scopes = ["read"] }]
            }
            : resourceSet with { Id = Id.Create() };

        if (!await resourceSetRepository.Add(subject, resourceSet, cancellationToken).ConfigureAwait(false))
        {
            return Results.Problem();
        }

        var response = new AddResourceSetResponse
        {
            Id = resourceSet.Id,
            UserAccessPolicyUri = $"{httpContext.Request.GetAbsoluteUriWithVirtualPath()}/{UmaConstants.RouteValues.ResourceSet}/{resourceSet.Id}/policy"
        };

        return Results.Json(response, statusCode: StatusCodes.Status201Created);
    }

    internal static async Task<IResult> UpdateResourceSet(
        HttpContext httpContext,
        ResourceSet resourceSet,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (resourceSet.IconUri != null && !resourceSet.IconUri.IsAbsoluteUri)
        {
            return BuildError(ErrorCodes.InvalidUri, Strings.TheUrlIsNotWellFormed, HttpStatusCode.BadRequest);
        }

        var updateResourceSet = new UpdateResourceSetAction(resourceSetRepository, loggerFactory.CreateLogger("DotAuth.Controllers.ResourceSetController"));
        var resourceSetUpdated = await updateResourceSet.Execute(resourceSet, cancellationToken).ConfigureAwait(false);
        if (resourceSetUpdated is Option.Error e)
        {
            return e.Details.Status switch
            {
                HttpStatusCode.BadRequest => Results.BadRequest(e.Details),
                _ => Results.Json(e.Details, statusCode: (int)e.Details.Status)
            };
        }

        return Results.Ok(new UpdateResourceSetResponse { Id = resourceSet.Id });
    }

    internal static async Task<IResult> DeleteResourceSet(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheIdentifierMustBeSpecified, HttpStatusCode.BadRequest);
        }

        var resourceSetExists = await resourceSetRepository.Remove(id, cancellationToken).ConfigureAwait(false);
        return !resourceSetExists
            ? Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidResourceSetId,
                Detail = ErrorCodes.InvalidResourceSetId,
                Status = HttpStatusCode.BadRequest
            })
            : Results.NoContent();
    }

    private static async Task<IResult?> SetResourceSetPolicy(
        HttpContext httpContext,
        string id,
        PolicyRule[] rules,
        IResourceSetRepository resourceSetRepository,
        CancellationToken cancellationToken)
    {
        var subject = httpContext.User.GetSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheSubjectCannotBeRetrieved, HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.TheIdentifierMustBeSpecified, HttpStatusCode.BadRequest);
        }

        var result = await resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return BuildError(ErrorCodes.InvalidRequest, Strings.InvalidResource, HttpStatusCode.BadRequest);
        }

        result = result with { AuthorizationPolicies = rules };
        var updated = await resourceSetRepository.Update(result, cancellationToken).ConfigureAwait(false);
        return updated switch
        {
            Option.Error => Results.Problem(),
            _ => null
        };
    }

    private static IResult BuildError(string code, string message, HttpStatusCode statusCode)
    {
        var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
        return Results.Json(error, statusCode: (int)statusCode);
    }

    private static PolicyRuleViewModel ToViewModel(PolicyRule rule)
    {
        return new()
        {
            Claims = rule.Claims,
            ClientIdsAllowed = string.Join(", ", rule.ClientIdsAllowed),
            IsResourceOwnerConsentNeeded = rule.IsResourceOwnerConsentNeeded,
            OpenIdProvider = rule.OpenIdProvider,
            Scopes = string.Join(", ", rule.Scopes)
        };
    }

    private static PolicyRule ToModel(PolicyRuleViewModel viewModel)
    {
        return new()
        {
            Scopes = viewModel.Scopes == null
                ? []
                : viewModel.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray(),
            Claims = viewModel.Claims.Where(x => !string.IsNullOrWhiteSpace(x.Type) && !string.IsNullOrWhiteSpace(x.Value)).ToArray(),
            ClientIdsAllowed = viewModel.ClientIdsAllowed == null
                ? []
                : viewModel.ClientIdsAllowed.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray(),
            IsResourceOwnerConsentNeeded = viewModel.IsResourceOwnerConsentNeeded,
            OpenIdProvider = viewModel.OpenIdProvider
        };
    }

    private static async Task<PolicyRule[]?> ReadPolicyRulesAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.Body == null)
        {
            return null;
        }

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        if (request.Body.CanSeek)
        {
            request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var serializerOptions = SharedSerializerContext.Default.Options;
        var editPolicyResponse = JsonSerializer.Deserialize<EditPolicyResponse>(payload, serializerOptions);
        if (editPolicyResponse?.Rules != null && editPolicyResponse.Rules.Length > 0)
        {
            return editPolicyResponse.Rules.Select(ToModel).ToArray();
        }

        return JsonSerializer.Deserialize<PolicyRule[]>(payload, serializerOptions);
    }
}




