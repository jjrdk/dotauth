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

using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Controllers
{
    using Api.Introspection;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the introspection controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(CoreConstants.EndPoints.Introspection)]
    public class IntrospectionController : ControllerBase
    {
        private readonly PostIntrospectionAction _introspectionActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectionController"/> class.
        /// </summary>
        /// <param name="clientStore">The client store.</param>
        /// <param name="tokenStore">The token store.</param>
        public IntrospectionController(IClientStore clientStore, ITokenStore tokenStore)
        {
            _introspectionActions = new PostIntrospectionAction(clientStore, tokenStore);
        }

        /// <summary>
        /// Handles the specified introspection request.
        /// </summary>
        /// <param name="introspectionRequest">The introspection request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] IntrospectionRequest introspectionRequest,
            CancellationToken cancellationToken)
        {
            if (introspectionRequest.token == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                authenticationHeaderValue = AuthenticationHeaderValue.Parse(authorizationHeader);
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _introspectionActions.Execute(
                    introspectionRequest.ToParameter(),
                    authenticationHeaderValue,
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            return new OkObjectResult(result);
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
}
