namespace DotAuth.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token.Actions;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Logging;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.WebSite.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

internal static class ResourceOwnersEndpointHandlers
{
    internal static async Task<IResult> GetAll(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var resourceOwners = await resourceOwnerRepository.GetAll(cancellationToken).ConfigureAwait(false);
        return Results.Ok(resourceOwners);
    }

    internal static async Task<IResult> Get(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var resourceOwner = await resourceOwnerRepository.Get(id, cancellationToken).ConfigureAwait(false);
        return resourceOwner == null
            ? Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Detail = Strings.TheRoDoesntExist,
                Title = ErrorCodes.InvalidRequest
            })
            : Results.Ok(resourceOwner);
    }

    internal static async Task<IResult> Delete(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (!await resourceOwnerRepository.Delete(id, cancellationToken).ConfigureAwait(false))
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.UnhandledExceptionCode,
                Detail = Strings.TheResourceOwnerCannotBeRemoved,
                Status = HttpStatusCode.BadRequest
            });
        }

        return Results.Ok();
    }

    internal static async Task<IResult> DeleteMe(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        ITokenStore tokenStore,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var sub = httpContext.User.GetSubject();
        if (sub == null)
        {
            return Results.BadRequest("Invalid user");
        }

        var value = httpContext.Request.Headers[HttpRequestHeader.Authorization.ToString()].FirstOrDefault();
        if (value == null)
        {
            return Results.BadRequest();
        }

        var accessToken = value.Split(' ').Last();
        var resourceOwner = await resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);

        if (await resourceOwnerRepository.Delete(sub, cancellationToken).ConfigureAwait(false)
            && await tokenStore.RemoveAccessToken(accessToken, cancellationToken).ConfigureAwait(false))
        {
            await eventPublisher.Publish(
                    new ResourceOwnerDeleted(
                        Id.Create(),
                        sub,
                        resourceOwner?.Claims == null
                            ? []
                            : resourceOwner.Claims.Select(x => new ClaimData { Type = x.Type, Value = x.Value }).ToArray(),
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            return Results.Ok();
        }

        return Results.BadRequest(new ErrorDetails
        {
            Title = ErrorCodes.UnhandledExceptionCode,
            Detail = Strings.TheResourceOwnerCannotBeRemoved,
            Status = HttpStatusCode.BadRequest
        });
    }

    internal static async Task<IResult> Update(
        HttpContext httpContext,
        string id,
        UpdateResourceOwnerClaimsRequest? request,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (request == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = Strings.ParameterInRequestBodyIsNotValid,
                Status = HttpStatusCode.BadRequest
            });
        }

        request = request with { Subject = id };
        var resourceOwner = await resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidParameterCode,
                Detail = Strings.TheRoDoesntExist
            });
        }

        var claims = request.Claims
            .Where(c => c.Type != OpenIdClaimTypes.Subject)
            .Where(c => c.Type != OpenIdClaimTypes.UpdatedAt)
            .Select(claim => new Claim(claim.Type, claim.Value))
            .Concat([
                new Claim(OpenIdClaimTypes.Subject, request.Subject),
                new Claim(OpenIdClaimTypes.UpdatedAt, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString())
            ]);

        resourceOwner.Claims = claims.ToArray();
        var result = await resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        return result switch
        {
            Option.Error => Results.BadRequest(Strings.TheClaimsCannotBeUpdated),
            _ => Results.Redirect($"/{CoreConstants.EndPoints.ResourceOwners}/{id}")
        };
    }

    internal static async Task<IResult> UpdateClaims(
        HttpContext httpContext,
        UpdateResourceOwnerClaimsRequest? request,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (request?.Subject == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.InvalidParameterCode,
                Detail = string.Format(Strings.MissingParameter, "login"),
                Status = HttpStatusCode.BadRequest
            });
        }

        var resourceOwner = await resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidParameterCode,
                Detail = Strings.TheRoDoesntExist
            });
        }

        var claims = request.Claims.Select(claim => new Claim(claim.Type, claim.Value)).ToList();
        var resourceOwnerClaims = resourceOwner.Claims
            .Where(c => !claims.Exists(x => x.Type == c.Type))
            .Concat(claims)
            .Where(c => c.Type != OpenIdClaimTypes.Subject)
            .Where(c => c.Type != OpenIdClaimTypes.UpdatedAt)
            .Concat([
                new Claim(OpenIdClaimTypes.Subject, request.Subject),
                new Claim(OpenIdClaimTypes.UpdatedAt, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString())
            ]);

        resourceOwner.Claims = resourceOwnerClaims.ToArray();
        var result = await resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        return result is Option.Success ? Results.Ok("{}") : Results.BadRequest(Strings.TheClaimsCannotBeUpdated);
    }

    internal static async Task<IResult> UpdateMyClaims(
        HttpContext httpContext,
        UpdateResourceOwnerClaimsRequest? request,
        IRequestThrottle requestThrottle,
        RuntimeSettings settings,
        ISubjectBuilder subjectBuilder,
        IResourceOwnerRepository resourceOwnerRepository,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IClientRepository clientRepository,
        IEnumerable<IAccountFilter> accountFilters,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (request == null)
        {
            return Results.BadRequest(Strings.NoParameterInBodyRequest);
        }

        var sub = httpContext.User.GetSubject();
        if (sub == null || sub != request.Subject)
        {
            return Results.BadRequest(Strings.InvalidUser);
        }

        var resourceOwner = await resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);
        var previousClaims = resourceOwner!.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }).ToArray();
        var newTypes = request.Claims.Select(x => x.Type).ToArray();
        resourceOwner.Claims = resourceOwner.Claims.Where(x => newTypes.All(n => n != x.Type))
            .Concat(request.Claims.Select(x => new Claim(x.Type, x.Value)))
            .ToArray();

        return await UpdateMyResourceOwner(
            httpContext,
            resourceOwner,
            previousClaims,
            resourceOwnerRepository,
            tokenStore,
            jwksStore,
            clientRepository,
            eventPublisher,
            cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> DeleteMyClaims(
        HttpContext httpContext,
        string[] type,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IClientRepository clientRepository,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var sub = httpContext.User.GetSubject();
        if (sub == null)
        {
            return Results.BadRequest(Strings.InvalidUser);
        }

        var resourceOwner = await resourceOwnerRepository.Get(sub, cancellationToken).ConfigureAwait(false);
        var previousClaims = resourceOwner!.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }).ToArray();
        var toDelete = type.Where(t => httpContext.User.HasClaim(x => x.Type == t)).ToArray();
        resourceOwner.Claims = resourceOwner.Claims.Where(x => !toDelete.Contains(x.Type)).ToArray();

        return await UpdateMyResourceOwner(
            httpContext,
            resourceOwner,
            previousClaims,
            resourceOwnerRepository,
            tokenStore,
            jwksStore,
            clientRepository,
            eventPublisher,
            cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task<IResult> UpdatePassword(
        HttpContext httpContext,
        UpdateResourceOwnerPasswordRequest? request,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (request?.Subject == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidParameterCode,
                Detail = string.Format(Strings.MissingParameter, "login")
            });
        }

        if (request.Password == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidParameterCode,
                Detail = string.Format(Strings.MissingParameter, "password")
            });
        }

        var resourceOwner = await resourceOwnerRepository.Get(request.Subject, cancellationToken).ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return Results.BadRequest(new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidParameterCode,
                Detail = Strings.TheRoDoesntExist
            });
        }

        var result = await resourceOwnerRepository.SetPassword(request.Subject, request.Password, cancellationToken).ConfigureAwait(false);
        return !result ? Results.BadRequest(Strings.ThePasswordCannotBeUpdated) : Results.Ok();
    }

    internal static async Task<IResult> Add(
        HttpContext httpContext,
        AddResourceOwnerRequest addResourceOwnerRequest,
        IRequestThrottle requestThrottle,
        RuntimeSettings settings,
        ISubjectBuilder subjectBuilder,
        IResourceOwnerRepository resourceOwnerRepository,
        IEnumerable<IAccountFilter> accountFilters,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var resourceOwner = new ResourceOwner
        {
            Subject = addResourceOwnerRequest.Subject ?? string.Empty,
            Password = addResourceOwnerRequest.Password,
            IsLocalAccount = true,
        };
        var addUserOperation = new AddUserOperation(settings, resourceOwnerRepository, accountFilters, subjectBuilder, eventPublisher);
        var (success, subject) = await addUserOperation.Execute(resourceOwner, cancellationToken).ConfigureAwait(false);
        return success
            ? Results.Ok(new SubjectResponse { Subject = subject })
            : Results.BadRequest(new ErrorDetails
            {
                Title = ErrorCodes.UnhandledExceptionCode,
                Detail = Strings.DuplicateResourceOwner,
                Status = HttpStatusCode.BadRequest
            });
    }

    internal static async Task<IResult> Search(
        HttpContext httpContext,
        SearchResourceOwnersRequest? searchResourceOwnersRequest,
        IRequestThrottle requestThrottle,
        IResourceOwnerRepository resourceOwnerRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        searchResourceOwnersRequest ??= new SearchResourceOwnersRequest { Descending = true, NbResults = 50, StartIndex = 0 };
        var result = await resourceOwnerRepository.Search(searchResourceOwnersRequest, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateMyResourceOwner(
        HttpContext httpContext,
        ResourceOwner resourceOwner,
        ClaimData[] previousClaims,
        IResourceOwnerRepository resourceOwnerRepository,
        ITokenStore tokenStore,
        IJwksStore jwksStore,
        IClientRepository clientRepository,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        var result = await resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
        if (!AuthenticationHeaderValue.TryParse(httpContext.Request.Headers[HeaderNames.Authorization], out var value))
        {
            return Results.BadRequest();
        }

        var accessToken = value.Parameter!;
        var existingToken = await tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
        if (existingToken == null)
        {
            return Results.BadRequest();
        }

        var client = await clientRepository.GetById(existingToken.ClientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            return Results.BadRequest();
        }

        var refreshOperation = new GetTokenByRefreshTokenGrantTypeAction(eventPublisher, tokenStore, jwksStore, resourceOwnerRepository, clientRepository);
        var refreshedResponse = await refreshOperation.Execute(
                new RefreshTokenGrantTypeParameter
                {
                    ClientId = existingToken.ClientId,
                    ClientSecret = client.Secrets[0].Value,
                    RefreshToken = existingToken.RefreshToken
                },
                null,
                httpContext.Request.GetCertificate(),
                httpContext.Request.GetAbsoluteUriWithVirtualPath(),
                cancellationToken)
            .ConfigureAwait(false);
        if (refreshedResponse is Option<GrantedToken>.Error error)
        {
            return Results.BadRequest(error.Details);
        }

        var refreshedToken = ((Option<GrantedToken>.Result)refreshedResponse).Item with
        {
            ParentTokenId = existingToken.ParentTokenId,
            RefreshToken = existingToken.RefreshToken
        };
        await tokenStore.RemoveAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
        await tokenStore.RemoveAccessToken(refreshedToken.AccessToken, cancellationToken).ConfigureAwait(false);
        await tokenStore.AddToken(refreshedToken, cancellationToken).ConfigureAwait(false);

        await eventPublisher.Publish(
                new ClaimsUpdated(
                    Id.Create(),
                    resourceOwner.Subject,
                    previousClaims,
                    resourceOwner.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }).ToArray(),
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);

        return result switch
        {
            Option.Error e => Results.BadRequest(e),
            _ => Results.Json(new GrantedTokenResponse
            {
                AccessToken = refreshedToken.AccessToken,
                ExpiresIn = refreshedToken.ExpiresIn,
                IdToken = refreshedToken.IdToken,
                RefreshToken = refreshedToken.RefreshToken,
                Scope = refreshedToken.Scope,
                TokenType = refreshedToken.TokenType
            })
        };
    }
}


