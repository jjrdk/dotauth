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
    using System;
    using System.Linq;
    using Api.ResourceSetController;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Responses;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Filters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    /// <summary>
    /// Defines the resource set controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(UmaConstants.RouteValues.ResourceSet)]
    [ThrottleFilter]
    public class ResourceSetController : ControllerBase
    {
        private const string NoParameterInBodyRequest = "no parameter in body request";
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly UpdateResourceSetAction _updateResourceSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSetController"/> class.
        /// </summary>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        public ResourceSetController(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
            _updateResourceSet = new UpdateResourceSetAction(resourceSetRepository);
        }

        /// <summary>
        /// Searches the resource sets.
        /// </summary>
        /// <param name="searchResourceSet">The search resource set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
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
                    Content = result.Content,
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
            return new OkObjectResult(resourceSets.Select(x => x.Id).ToArray());
        }

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetResourceSet(string id, CancellationToken cancellationToken)
        {
            var subject = User.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.TheSubjectCannotBeRetrieved,
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return NoContent();
            }

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets the access policy definition for the given resource.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async request.</param>
        /// <returns></returns>
        [HttpGet("{id}/policy")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> GetResourceSetPolicy(string id, CancellationToken cancellationToken)
        {
            var subject = User.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.TheSubjectCannotBeRetrieved,
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return BadRequest();
            }

            return new OkObjectResult(new EditPolicyResponse { Id = id, Rules = result.AuthorizationPolicies.Select(ToViewModel).ToArray() });
        }

        /// <summary>
        /// Sets the access policy definition for the given resource.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async request.</param>
        [HttpPost("{id}/policy")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> SetResourceSetPolicy(
            EditPolicyResponse viewModel,
            CancellationToken cancellationToken)
        {
            if (viewModel == null)
            {
                return BadRequest(
                    new ErrorDetails
                    {
                        Detail = "Input missing",
                        Title = "Input missing",
                        Status = HttpStatusCode.BadRequest
                    });
            }
            var result = await SetResourceSetPolicy(viewModel.Id, viewModel.Rules.Select(ToModel).ToArray(), cancellationToken).ConfigureAwait(false);

            return result is OkResult ? Ok(new object()) : result;
        }

        /// <summary>
        /// Sets the access policy definition for the given resource.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <param name="rules">The access policy rules to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async request.</param>
        [HttpPut("{id}/policy")]
        [Authorize(Policy = "UmaProtection")]
        public async Task<IActionResult> SetResourceSetPolicy(string id, PolicyRule[] rules, CancellationToken cancellationToken)
        {
            var subject = User.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.TheSubjectCannotBeRetrieved,
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "the identifier must be specified",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceSetRepository.Get(subject, id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    "invalid resource",
                    HttpStatusCode.BadRequest);
            }

            result.AuthorizationPolicies = rules;
            var updated = await _resourceSetRepository.Update(result, cancellationToken).ConfigureAwait(false);

            return updated ? Ok() : (IActionResult)Problem();
        }

        /// <summary>
        /// Adds the resource set.
        /// </summary>
        /// <param name="resourceSet">The post resource set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("UmaProtection")]
        public async Task<IActionResult> AddResourceSet(
            [FromBody] ResourceSet resourceSet,
            CancellationToken cancellationToken)
        {
            var subject = User.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.TheSubjectCannotBeRetrieved,
                    HttpStatusCode.BadRequest);
            }

            if (resourceSet == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    NoParameterInBodyRequest,
                    HttpStatusCode.BadRequest);
            }

            if (resourceSet.IconUri != null && !resourceSet.IconUri.IsAbsoluteUri)
            {
                return BuildError(
                    ErrorCodes.InvalidUri,
                    ErrorDescriptions.TheUrlIsNotWellFormed,
                    HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(resourceSet.Name))
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"),
                    HttpStatusCode.BadRequest);
            }

            if (resourceSet.Scopes == null || !resourceSet.Scopes.Any())
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"),
                    HttpStatusCode.BadRequest);
            }

            resourceSet.Id = Id.Create();
            resourceSet.AuthorizationPolicies = new[] { new PolicyRule { IsResourceOwnerConsentNeeded = true } };
            if (!await _resourceSetRepository.Add(subject, resourceSet, cancellationToken).ConfigureAwait(false))
            {
                return Problem();
            }

            var response = new AddResourceSetResponse
            {
                Id = resourceSet.Id,
                UserAccessPolicyUri =
                    $"{Request.GetAbsoluteUriWithVirtualPath()}/{UmaConstants.RouteValues.ResourceSet}/{resourceSet.Id}/policy"
            };

            return new ObjectResult(response) { StatusCode = (int)HttpStatusCode.Created };
        }

        /// <summary>
        /// Updates the resource set.
        /// </summary>
        /// <param name="resourceSet">The put resource set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
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

            if (resourceSet.IconUri != null && !resourceSet.IconUri.IsAbsoluteUri)
            {
                return BuildError(
                    ErrorCodes.InvalidUri,
                    ErrorDescriptions.TheUrlIsNotWellFormed,
                    HttpStatusCode.BadRequest);
            }

            var resourceSetUpdated =
                await _updateResourceSet.Execute(resourceSet, cancellationToken).ConfigureAwait(false);
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

            var resourceSetExists = await _resourceSetRepository.Remove(id, cancellationToken).ConfigureAwait(false);
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

        private static PolicyRuleViewModel ToViewModel(PolicyRule rule)
        {
            return new PolicyRuleViewModel
            {
                Claims = rule.Claims,
                ClientIdsAllowed = string.Join(", ", rule.ClientIdsAllowed),
                IsResourceOwnerConsentNeeded = rule.IsResourceOwnerConsentNeeded,
                OpenIdProvider = rule.OpenIdProvider,
                Scopes = string.Join(", ", rule.Scopes)
            };
        }

        private static PolicyRule ToModel(PolicyRuleViewModel viewModel)
        {
            return new PolicyRule
            {
                Scopes =
                    viewModel.Scopes?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray(),
                Claims = viewModel.Claims ?? Array.Empty<ClaimData>(),
                ClientIdsAllowed =
                    viewModel.ClientIdsAllowed?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray(),
                IsResourceOwnerConsentNeeded = viewModel.IsResourceOwnerConsentNeeded,
                OpenIdProvider = viewModel.OpenIdProvider
            };
        }
    }
}
