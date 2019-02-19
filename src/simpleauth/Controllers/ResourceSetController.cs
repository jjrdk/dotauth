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
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Api.ResourceSetController;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Repositories;
    using Shared.DTOs;
    using Shared.Responses;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the resource set controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.ResourceSet)]
    public class ResourceSetController : Controller
    {
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
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize("UmaProtection")]
        public async Task<ActionResult<GenericResult<ResourceSet>>> SearchResourceSets([FromBody] SearchResourceSet searchResourceSet)
        {
            if (searchResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Search(searchResourceSet).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets the resource sets.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> GetResourceSets()
        {
            var resourceSets = await _resourceSetRepository.GetAll().ConfigureAwait(false);
            var resourceSetIds = resourceSets.Select(x => x.Id).ToArray();
            return new OkObjectResult(resourceSetIds);
        }

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> GetResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Get(id).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundResourceSet();
            }

            var content = result.ToResponse();
            return new OkObjectResult(content);
        }

        /// <summary>
        /// Adds the resource set.
        /// </summary>
        /// <param name="postResourceSet">The post resource set.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> AddResourceSet([FromBody] PostResourceSet postResourceSet)
        {
            if (postResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var result = await _addResourceSet.Execute(postResourceSet).ConfigureAwait(false);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        /// <summary>
        /// Updates the resource set.
        /// </summary>
        /// <param name="putResourceSet">The put resource set.</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> UpdateResourceSet([FromBody] PutResourceSet putResourceSet)
        {
            if (putResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var resourceSetExists = await _updateResourceSet.Execute(putResourceSet).ConfigureAwait(false);
            if (!resourceSetExists)
            {
                return GetNotFoundResourceSet();
            }

            var response = new UpdateResourceSetResponse
            {
                Id = putResourceSet.Id
            };

            return new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Deletes the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> DeleteResourceSet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var resourceSetExists = await _removeResourceSet.Execute(id).ConfigureAwait(false);
            return !resourceSetExists
                ? GetNotFoundResourceSet()
                : new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundResourceSet()
        {
            var errorResponse = new ErrorResponse
            {
                Error = "not_found",
                ErrorDescription = "resource cannot be found"
            };

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return new JsonResult(error)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
