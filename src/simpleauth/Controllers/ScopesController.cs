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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Repositories;
    using Shared.Requests;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the scopes controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(CoreConstants.EndPoints.Scopes)]
    public class ScopesController : ControllerBase
    {
        private readonly IScopeRepository _scopeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopesController"/> class.
        /// </summary>
        /// <param name="scopeRepository">The scope repository.</param>
        public ScopesController(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }

        /// <summary>
        /// Searches the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        [HttpPost(".search")]
        [Authorize(Policy = "manager")]
        public async Task<ActionResult<GenericResult<Scope>>> Search(
            [FromBody] SearchScopesRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                BadRequest();
            }

            var result = await _scopeRepository.Search(request, cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _scopeRepository.GetAll(cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<ActionResult<Scope>> Get(string id, CancellationToken cancellationToken)
        {
            return await _scopeRepository.Get(id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">id</exception>
        [HttpDelete("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var scope = await _scopeRepository.Get(id, cancellationToken).ConfigureAwait(false);
            if (scope == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequestCode,
                        Detail = string.Format(ErrorDescriptions.TheScopeDoesntExist, id),
                        Status = HttpStatusCode.BadRequest
                    });
            }

            var deleted = await _scopeRepository.Delete(scope, CancellationToken.None).ConfigureAwait(false);

            return deleted
                ? (IActionResult) NoContent()
                : BadRequest(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequestCode,
                        Detail = string.Format(ErrorDescriptions.TheScopeDoesntExist, id),
                        Status = HttpStatusCode.BadRequest
                    });
        }

        /// <summary>
        /// Adds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        [HttpPost]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Add([FromBody] Scope request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return !await _scopeRepository.Insert(request, CancellationToken.None).ConfigureAwait(false)
                ? new StatusCodeResult(StatusCodes.Status500InternalServerError)
                : new NoContentResult();
        }

        /// <summary>
        /// Updates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">request</exception>
        [HttpPut]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Update([FromBody] Scope request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return !await _scopeRepository.Update(request, cancellationToken).ConfigureAwait(false)
                ? new StatusCodeResult(StatusCodes.Status500InternalServerError)
                : new NoContentResult();
        }
    }
}
