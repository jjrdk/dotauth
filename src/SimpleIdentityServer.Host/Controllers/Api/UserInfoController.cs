// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Host.Controllers.Api
{
    using Core.Api.UserInfo;
    using Core.Errors;
    using Core.Exceptions;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    [Route(Core.Constants.EndPoints.UserInfo)]
    public class UserInfoController : Controller
    {
        private readonly IUserInfoActions _userInfoActions;

        public UserInfoController(IUserInfoActions userInfoActions)
        {
            _userInfoActions = userInfoActions;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return await ProcessRequest().ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            return await ProcessRequest().ConfigureAwait(false);
        }

        private async Task<ActionResult> ProcessRequest()
        {
            var accessToken = await TryToGetTheAccessToken().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
            }

            var result = await _userInfoActions.GetUserInformation(accessToken).ConfigureAwait(false);
            return result.Content;
        }

        private async Task<string> TryToGetTheAccessToken()
        {
            var accessToken = GetAccessTokenFromAuthorizationHeader();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            accessToken = await GetAccessTokenFromBodyParameter().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            return GetAccessTokenFromQueryString();
        }

        /// <summary>
        /// Get an access token from the authorization header.
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromAuthorizationHeader()
        {
            const string authorizationName = "Authorization";
            if (!Request.Headers.TryGetValue(authorizationName, out var values))
            {
                return string.Empty;
            }

            var authenticationHeader = values.First();
            var authorization = AuthenticationHeaderValue.Parse(authenticationHeader);
            var scheme = authorization.Scheme;
            if (string.Compare(scheme, "Bearer", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return string.Empty;
            }

            return authorization.Parameter;
        }

        /// <summary>
        /// Get an access token from the body parameter.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenFromBodyParameter()
        {
            const string contentTypeName = "Content-Type";
            const string contentTypeValue = "application/x-www-form-urlencoded";
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var emptyResult = string.Empty;
            if (Request.Headers == null
                || !Request.Headers.TryGetValue(contentTypeName, out var values))
            {
                return emptyResult;
            }

            var contentTypeHeader = values.First();
            if (string.Compare(contentTypeHeader, contentTypeValue) != 0)
            {
                return emptyResult;
            }

            var content = await Request.ReadAsStringAsync().ConfigureAwait(false);
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(content);
            if (!queryString.Keys.Contains(accessTokenName))
            {
                return emptyResult;
            }

            queryString.TryGetValue(accessTokenName, out var result);
            return result.First();
        }

        /// <summary>
        /// Get an access token from the query string
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromQueryString()
        {
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var query = Request.Query;
            var record = query.FirstOrDefault(q => q.Key == accessTokenName);
            if (record.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return string.Empty;
            }
            else
            {
                return record.Value.First();
            }
        }
    }
}