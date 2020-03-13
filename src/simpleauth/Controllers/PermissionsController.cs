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

namespace SimpleAuth.Controllers
{
    using Api.PermissionController;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.DTOs;
    using Shared.Responses;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    /// <summary>
    /// Defines the permission controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.Permission)]
    public class PermissionsController : ControllerBase
    {
        private readonly ITicketStore _ticketStore;
        private readonly AddPermissionAction _requestPermission;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsController"/> class.
        /// </summary>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        /// <param name="ticketStore">The ticket store.</param>
        /// <param name="options">The options.</param>
        public PermissionsController(
            IResourceSetRepository resourceSetRepository,
            ITicketStore ticketStore,
            RuntimeSettings options)
        {
            _ticketStore = ticketStore;
            _requestPermission = new AddPermissionAction(resourceSetRepository, ticketStore, options);
        }

        [HttpGet]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetPermissionRequests(CancellationToken cancellationToken)
        {
            var owner = User.GetSubject();
            var tickets = await _ticketStore.GetAll(owner, cancellationToken).ConfigureAwait(false);

            return new OkObjectResult(tickets.Select(x => !x.IsAuthorizedByRo).ToArray());
        }

        /// <summary>
        /// Adds the permission.
        /// </summary>
        /// <param name="permissionRequest">The post permission.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> PostPermission(
            [FromBody] PermissionRequest permissionRequest,
            CancellationToken cancellationToken)
        {
            if (permissionRequest == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(permissionRequest.ResourceSetId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the parameter resource_set_id needs to be specified",
                    HttpStatusCode.BadRequest);
            }

            var clientId = this.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the client_id cannot be extracted",
                    HttpStatusCode.BadRequest);
            }

            var ticketId = await _requestPermission.Execute(clientId, cancellationToken, permissionRequest)
                .ConfigureAwait(false);
            var result = new PermissionResponse { TicketId = ticketId };
            return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
        }

        /// <summary>
        /// Adds the permissions.
        /// </summary>
        /// <param name="postPermissions">The post permissions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost("bulk")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> PostPermissions(
            [FromBody] PermissionRequest[] postPermissions,
            CancellationToken cancellationToken)
        {
            if (postPermissions == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var parameters = postPermissions.ToArray();
            var clientId = this.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the client_id cannot be extracted",
                    HttpStatusCode.BadRequest);
            }

            var ticketId = await _requestPermission.Execute(clientId, cancellationToken, parameters)
                .ConfigureAwait(false);
            var result = new PermissionResponse { TicketId = ticketId };
            return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
        }

        private static IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message };
            return new BadRequestObjectResult(error);
        }
    }
}
