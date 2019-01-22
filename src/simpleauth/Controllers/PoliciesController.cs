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

using SimpleAuth.Api.PolicyController.Actions;
using SimpleAuth.Repositories;

namespace SimpleAuth.Controllers
{
    using Errors;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Parameters;
    using Shared.DTOs;
    using Shared.Responses;
    using System.Net;
    using System.Threading.Tasks;

    [Route(UmaConstants.RouteValues.Policies)]
    public class PoliciesController : Controller
    {
        private readonly AddAuthorizationPolicyAction _addpolicy;
        private readonly DeleteAuthorizationPolicyAction _deletePolicy;
        private readonly DeleteResourcePolicyAction _deleteResourceSet;
        private readonly AddResourceSetToPolicyAction _addResourceSet;
        private readonly UpdatePolicyAction _updatePolicy;
        private readonly GetAuthorizationPolicyAction _getPolicy;
        private readonly GetAuthorizationPoliciesAction _getPolicies;
        private readonly SearchAuthPoliciesAction _searchPolicy;

        public PoliciesController(IPolicyRepository policyRepository, IResourceSetRepository resourceSetRepository)
        {
            _addpolicy = new AddAuthorizationPolicyAction(policyRepository, resourceSetRepository);
            _deletePolicy = new DeleteAuthorizationPolicyAction(policyRepository);
            _addResourceSet = new AddResourceSetToPolicyAction(policyRepository, resourceSetRepository);
            _deleteResourceSet = new DeleteResourcePolicyAction(policyRepository, resourceSetRepository);
            _updatePolicy = new UpdatePolicyAction(policyRepository, resourceSetRepository);
            _getPolicy = new GetAuthorizationPolicyAction(policyRepository);
            _getPolicies = new GetAuthorizationPoliciesAction(policyRepository);
            _searchPolicy = new SearchAuthPoliciesAction(policyRepository);
        }

        [HttpPost(".search")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> SearchPolicies([FromBody] SearchAuthPolicies searchAuthPolicies)
        {
            if (searchAuthPolicies == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = searchAuthPolicies.ToParameter();
            var result = await _searchPolicy.Execute(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToResponse());
        }

        [HttpGet("{id}")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> GetPolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            var result = await _getPolicy.Execute(id).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id);
            return new OkObjectResult(content);
        }

        [HttpGet]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> GetPolicies()
        {
            var policies = await _getPolicies.Execute().ConfigureAwait(false);
            return new OkObjectResult(policies);
        }

        // Partial update
        [HttpPut]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> PutPolicy([FromBody] PutPolicy putPolicy)
        {
            if (putPolicy == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _updatePolicy.Execute(putPolicy.ToParameter()).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpPost("{id}/resources")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> PostAddResourceSet(string id, [FromBody] PostAddResourceSet postAddResourceSet)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (postAddResourceSet == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _addResourceSet.Execute(
                new AddResourceSetParameter
                {
                    PolicyId = id,
                    ResourceSets = postAddResourceSet.ResourceSets
                }).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpDelete("{id}/resources/{resourceId}")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> DeleteResourceSet(string id, string resourceId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the resource_id must be specified", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _deleteResourceSet.Execute(id, resourceId).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> PostPolicy([FromBody] PostPolicy postPolicy)
        {
            if (postPolicy == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var policyId = await _addpolicy.Execute(postPolicy.ToParameter()).ConfigureAwait(false);
            var content = new AddPolicyResponse
            {
                PolicyId = policyId
            };

            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPoliciesStoreName, false);
            return new ObjectResult(content)
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpDelete("{id}")]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> DeletePolicy(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the identifier must be specified", HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _deletePolicy.Execute(id).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPolicyStoreName + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, CachingStoreNames.GetPoliciesStoreName, false);
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundPolicy()
        {
            var errorResponse = new ErrorResponse
            {
                Error = "not_found",
                ErrorDescription = "policy cannot be found"
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
