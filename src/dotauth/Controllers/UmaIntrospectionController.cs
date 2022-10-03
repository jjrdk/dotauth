namespace DotAuth.Controllers;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Introspection;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines the introspection controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(UmaConstants.RouteValues.Introspection)]
[ThrottleFilter]
public sealed class UmaIntrospectionController : ControllerBase
{
    private readonly UmaIntrospectionAction _introspectionActions;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntrospectionController"/> class.
    /// </summary>
    /// <param name="tokenStore">The token store.</param>
    public UmaIntrospectionController(ITokenStore tokenStore)
    {
        _introspectionActions = new UmaIntrospectionAction(tokenStore);
    }

    /// <summary>
    /// Handles the specified introspection request.
    /// </summary>
    /// <param name="introspectionRequest">The introspection request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post(
        [FromForm] IntrospectionRequest introspectionRequest,
        CancellationToken cancellationToken)
    {
        if (introspectionRequest?.token == null)
        {
            return BuildError(
                ErrorCodes.InvalidRequest,
                Strings.NoParameterInBodyRequest,
                HttpStatusCode.BadRequest);
        }

        var result = await _introspectionActions.Execute(
                introspectionRequest.ToParameter(),
                cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Option<UmaIntrospectionResponse>.Result r => Ok(r.Item),
            Option<UmaIntrospectionResponse>.Error e => BadRequest(e.Details),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Build the JSON error message.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
    {
        var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
        return new JsonResult(error) { StatusCode = (int)statusCode };
    }
}