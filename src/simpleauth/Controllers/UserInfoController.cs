﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SimpleAuth.Filters;
using SimpleAuth.Properties;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Repositories;

/// <summary>
/// Endpoint for user info introspection requests.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(CoreConstants.EndPoints.UserInfo)]
[ThrottleFilter]
public sealed class UserInfoController : ControllerBase
{
    private readonly ITokenStore _tokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoController"/> class.
    /// </summary>
    /// <param name="tokenStore">The token store.</param>
    public UserInfoController(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Handles the user info introspection request.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var accessToken = await TryToGetTheAccessToken().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return BadRequest(new ErrorDetails { Title = ErrorCodes.InvalidToken });
        }

        var grantedToken = await _tokenStore.GetAccessToken(accessToken, cancellationToken).ConfigureAwait(false);
        return grantedToken == null
            ? BadRequest(
                new ErrorDetails { Detail = Strings.TheTokenIsNotValid, Title = ErrorCodes.InvalidToken })
            : new ObjectResult(grantedToken.UserInfoPayLoad ?? grantedToken.IdTokenPayLoad ?? new JwtPayload());
    }

    private async Task<string> TryToGetTheAccessToken()
    {
        var accessToken = GetAccessTokenFromAuthorizationHeader();
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return accessToken;
        }

        accessToken = await GetAccessTokenFromBodyParameter().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(accessToken) ? GetAccessTokenFromQueryString() : accessToken;
    }

    /// <summary>
    /// Get an access token from the authorization header.
    /// </summary>
    /// <returns></returns>
    private string GetAccessTokenFromAuthorizationHeader()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var values))
        {
            return string.Empty;
        }

        var authenticationHeader = values.First();
        var authorization = AuthenticationHeaderValue.Parse(authenticationHeader);
        var scheme = authorization.Scheme;
        return authorization.Parameter == null || string.Compare(scheme, "Bearer", StringComparison.CurrentCultureIgnoreCase) != 0
            ? string.Empty
            : authorization.Parameter;
    }

    /// <summary>
    /// Get an access token from the body parameter.
    /// </summary>
    /// <returns></returns>
    private async Task<string?> GetAccessTokenFromBodyParameter()
    {
        if (!Request.HasFormContentType)
        {
            return null;
        }

        var content = await Request.ReadFormAsync().ConfigureAwait(false);

        return !content.TryGetValue(StandardAuthorizationResponseNames.AccessTokenName, out var result)
            ? null
            : result.First();
    }

    /// <summary>
    /// Get an access token from the query string
    /// </summary>
    /// <returns></returns>
    private string GetAccessTokenFromQueryString()
    {
        var accessTokenName = StandardAuthorizationResponseNames.AccessTokenName;
        var query = Request.Query;
        var record = query.FirstOrDefault(q => q.Key == accessTokenName);
        return record.Equals(default(KeyValuePair<string, StringValues>)) ? string.Empty : record.Value.First()!;
    }
}