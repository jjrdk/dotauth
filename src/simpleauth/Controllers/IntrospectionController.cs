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

namespace SimpleAuth.Controllers;

using System;
using Api.Introspection;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Requests;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SimpleAuth.Filters;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.Shared.Responses;

/// <summary>
/// Defines the introspection controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(CoreConstants.EndPoints.Introspection)]
[ThrottleFilter]
public sealed class IntrospectionController : ControllerBase
{
    private readonly PostIntrospectionAction _introspectionActions;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntrospectionController"/> class.
    /// </summary>
    /// <param name="tokenStore">The token store.</param>
    public IntrospectionController(ITokenStore tokenStore)
    {
        _introspectionActions = new PostIntrospectionAction(tokenStore);
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
                "no parameter in body request",
                HttpStatusCode.BadRequest);
        }

        var result = await _introspectionActions.Execute(
                introspectionRequest.ToParameter(),
                cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Option<OauthIntrospectionResponse>.Result r => Ok(r.Item),
            Option<OauthIntrospectionResponse>.Error e => BadRequest(e.Details),
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