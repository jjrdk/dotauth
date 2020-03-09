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
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.DTOs;
    using Shared.Responses;
    using SimpleAuth.Api.PolicyController;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the policies controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route(UmaConstants.RouteValues.Policies)]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly AddAuthorizationPolicyAction _addpolicy;
        private readonly DeleteAuthorizationPolicyAction _deletePolicy;
        private readonly DeleteResourcePolicyAction _deleteResourceSet;
        private readonly UpdatePolicyAction _updatePolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoliciesController"/> class.
        /// </summary>
        /// <param name="policyRepository">The policy repository.</param>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        public PoliciesController(IPolicyRepository policyRepository, IResourceSetRepository resourceSetRepository)
        {
            _policyRepository = policyRepository;
            _addpolicy = new AddAuthorizationPolicyAction(policyRepository);
            _deletePolicy = new DeleteAuthorizationPolicyAction(policyRepository);
            _deleteResourceSet = new DeleteResourcePolicyAction(policyRepository, resourceSetRepository);
            _updatePolicy = new UpdatePolicyAction(policyRepository);
        }

        /// <summary>
        /// Searches the policies.
        /// </summary>
        /// <param name="searchAuthPolicies">The search authentication policies.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> SearchPolicies(
            [FromBody] SearchAuthPolicies searchAuthPolicies,
            CancellationToken cancellationToken)
        {
            if (searchAuthPolicies == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var result = await _policyRepository.Search(searchAuthPolicies, cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result.ToResponse());
        }

        /// <summary>
        /// Gets the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetPolicy(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _policyRepository.Get(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return GetNotFoundPolicy();
            }

            var content = result.ToResponse();
            return new OkObjectResult(content);
        }

        /// <summary>
        /// Gets the policies.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetPolicies(CancellationToken cancellationToken)
        {
            var owner = User.GetSubject();
            var policies = await _policyRepository.GetAll(owner, cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(policies.Select(p => p.ToResponse()).ToArray());
        }

        /// <summary>
        /// Updates the policy.
        /// </summary>
        /// <param name="putPolicy">The put policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> PutPolicy([FromBody] PolicyData putPolicy, CancellationToken cancellationToken)
        {
            if (putPolicy == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(putPolicy.PolicyId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the parameter id needs to be specified",
                    HttpStatusCode.BadRequest);
            }

            if (putPolicy.Rules == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the parameter rules needs to be specified",
                    HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _updatePolicy.Execute(putPolicy, cancellationToken).ConfigureAwait(false);
            return !isPolicyExists ? GetNotFoundPolicy() : new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete("{id}/resources/{resourceId}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> DeleteResourceSet(
            string id,
            string resourceId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the resource_id must be specified",
                    HttpStatusCode.BadRequest);
            }

            var isPolicyExists =
                await _deleteResourceSet.Execute(id, resourceId, cancellationToken).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Adds the policy.
        /// </summary>
        /// <param name="postPolicy">The post policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> PostPolicy(
            [FromBody] PolicyData postPolicy,
            CancellationToken cancellationToken)
        {
            if (postPolicy == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var owner = User.GetSubject();
            var policyId = await _addpolicy.Execute(owner, postPolicy, cancellationToken).ConfigureAwait(false);
            var content = new AddPolicyResponse { PolicyId = policyId };

            return new ObjectResult(content) { StatusCode = (int)HttpStatusCode.Created };
        }

        /// <summary>
        /// Deletes the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> DeletePolicy(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var isPolicyExists = await _deletePolicy.Execute(id, cancellationToken).ConfigureAwait(false);
            if (!isPolicyExists)
            {
                return GetNotFoundPolicy();
            }

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private static ActionResult GetNotFoundPolicy()
        {
            var errorResponse = new ErrorDetails { Title = "not_found", Detail = "policy cannot be found" };

            return new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.NotFound };
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
            return new JsonResult(error) { StatusCode = (int)statusCode };
        }
    }
}
