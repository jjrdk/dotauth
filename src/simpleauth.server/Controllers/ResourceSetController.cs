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

namespace SimpleAuth.Server.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Api.ResourceSetController;
    using Errors;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.DTOs;
    using Shared.Responses;

    [Route(UmaConstants.RouteValues.ResourceSet)]
    public class ResourceSetController : Controller
    {
        private readonly IResourceSetActions _resourceSetActions;

        public ResourceSetController(
            IResourceSetActions resourceSetActions)
        {
            _resourceSetActions = resourceSetActions;
        }

        [HttpPost(".search")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> SearchResourceSets([FromBody] SearchResourceSet searchResourceSet)
        {
            if (searchResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var parameter = searchResourceSet.ToParameter();
            var result = await _resourceSetActions.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToResponse());
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> GetResourceSets()
        {
            var resourceSetIds = await _resourceSetActions.GetAllResourceSet().ConfigureAwait(false);
            return new OkObjectResult(resourceSetIds);
        }

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

            var result = await _resourceSetActions.GetResourceSet(id).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundResourceSet();
            }

            var content = result.ToResponse();
            return new OkObjectResult(content);
        }

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

            var parameter = postResourceSet.ToParameter();
            var result = await _resourceSetActions.AddResourceSet(parameter).ConfigureAwait(false);
            var response = new AddResourceSetResponse
            {
                Id = result
            };
            return new ObjectResult(response)
            {
                StatusCode = (int) HttpStatusCode.Created
            };
        }

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

            var parameter = putResourceSet.ToParameter();
            var resourceSetExists = await _resourceSetActions.UpdateResourceSet(parameter).ConfigureAwait(false);
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
                StatusCode = (int) HttpStatusCode.OK
            };
        }

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

            var resourceSetExists = await _resourceSetActions.RemoveResourceSet(id).ConfigureAwait(false);
            return !resourceSetExists 
                ? GetNotFoundResourceSet() 
                : new StatusCodeResult((int) HttpStatusCode.NoContent);
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
                StatusCode = (int) HttpStatusCode.NotFound
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
                StatusCode = (int) statusCode
            };
        }
    }
}
