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
    using Api.ResourceSetController;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.DTOs;
    using Shared.Responses;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the resource set controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.ResourceSet)]
    public class ResourceSetController : ControllerBase
    {
        private const string NoParameterInBodyRequest = "no parameter in body request";
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly AddResourceSetAction _addResourceSet;
        private readonly UpdateResourceSetAction _updateResourceSet;
        private readonly DeleteResourceSetAction _removeResourceSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSetController"/> class.
        /// </summary>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        public ResourceSetController(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
            _addResourceSet = new AddResourceSetAction(resourceSetRepository);
            _updateResourceSet = new UpdateResourceSetAction(resourceSetRepository);
            _removeResourceSet = new DeleteResourceSetAction(resourceSetRepository);
        }

        /// <summary>
        /// Searches the resource sets.
        /// </summary>
        /// <param name="searchResourceSet">The search resource set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult<PagedResult<ResourceSet>>> SearchResourceSets(
            [FromBody] SearchResourceSet searchResourceSet,
            CancellationToken cancellationToken)
        {
            if (searchResourceSet == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Search(searchResourceSet, cancellationToken)
                .ConfigureAwait(false);
            return new OkObjectResult(
                new PagedResult<ResourceSet>
                {
                    Content = result.Content.Select(x => x.ToResponse()).ToArray(),
                    StartIndex = result.StartIndex,
                    TotalResults = result.TotalResults
                });
        }

        /// <summary>
        /// Gets the resource sets.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetResourceSets(CancellationToken cancellationToken)
        {
            var owner = User.GetSubject();
            if (string.IsNullOrWhiteSpace(owner))
            {
                return BadRequest();
            }
            var resourceSets = await _resourceSetRepository.GetAll(owner, cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(resourceSets.Select(x=>x.ToResponse()).ToArray());
        }

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetResourceSet(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Get(id, cancellationToken).ConfigureAwait(false);
            if (result == null || result.Owner != User.GetSubject())
            {
                return BadRequest();
            }

            var content = result.ToResponse();
            return new OkObjectResult(content);
        }

        /// <summary>
        /// Adds the resource set.
        /// </summary>
        /// <param name="postResourceSet">The post resource set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> AddResourceSet(
            [FromBody] ResourceSet postResourceSet,
            CancellationToken cancellationToken)
        {
            if (postResourceSet == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }

            var owner = User.GetSubject();
            if (string.IsNullOrWhiteSpace(owner))
            {
                return BadRequest("subject not defined");
            }
            postResourceSet.Id = Id.Create();
            var result = await _addResourceSet.Execute(owner, postResourceSet, cancellationToken).ConfigureAwait(false);
            var response = new AddResourceSetResponse { Id = result };
            return new ObjectResult(response) { StatusCode = (int)HttpStatusCode.Created };
        }

        /// <summary>
        /// Updates the resource set.
        /// </summary>
        /// <param name="resourceSet">The put resource set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> UpdateResourceSet(
            [FromBody] ResourceSet resourceSet,
            CancellationToken cancellationToken)
        {
            if (resourceSet == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }

            var owner = User.GetSubject();
            var resourceSetUpdated =
                await _updateResourceSet.Execute(owner, resourceSet, cancellationToken).ConfigureAwait(false);
            if (!resourceSetUpdated)
            {
                return GetNotUpdatedResourceSet();
            }

            var response = new UpdateResourceSetResponse { Id = resourceSet.Id };

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Deletes the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> DeleteResourceSet(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var resourceSetExists = await _removeResourceSet.Execute(id, cancellationToken).ConfigureAwait(false);
            return !resourceSetExists
                ? (IActionResult)BadRequest(new ErrorDetails { Status = HttpStatusCode.BadRequest })
                : NoContent();
        }

        private static ActionResult GetNotUpdatedResourceSet()
        {
            var errorResponse = new ErrorDetails
            {
                Status = HttpStatusCode.NotFound,
                Title = "not_updated",
                Detail = "resource cannot be updated"
            };

            return new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.NotFound };
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
            return new JsonResult(error) { StatusCode = (int)statusCode };
        }
    }
}
