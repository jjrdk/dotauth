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
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using WebSite.User.Actions;

    [Route(CoreConstants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : Controller
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly AddUserOperation _addUserOperation;

        public ResourceOwnersController(
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<AccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _addUserOperation = new AddUserOperation(resourceOwnerRepository, accountFilters, eventPublisher);
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var content = (await _resourceOwnerRepository.GetAll(cancellationToken).ConfigureAwait(false)).ToDtos();
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var resourceOwner = await _resourceOwnerRepository.Get(id, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, id));
            }

            return Ok(resourceOwner.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (!await _resourceOwnerRepository.Delete(id, cancellationToken).ConfigureAwait(false))
            {
                return BadRequest(
                    new ErrorResponse
                    {
                        Error = ErrorCodes.UnhandledExceptionCode,
                        ErrorDescription = ErrorDescriptions.TheResourceOwnerCannotBeRemoved
                    });
            }

            return Ok();
        }

        [HttpPut("claims")]
        [Authorize("manager")]
        public async Task<IActionResult> UpdateClaims(
            [FromBody] UpdateResourceOwnerClaimsRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }


            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Login, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.UpdateDateTime = DateTime.UtcNow;
            var claims = request.Claims.Select(claim => new Claim(claim.Key, claim.Value)).ToArray();

            resourceOwner.Claims = claims;
            Claim updatedClaim;
            if ((updatedClaim = resourceOwner.Claims.FirstOrDefault(
                    c => c.Type == JwtConstants.OpenIdClaimTypes.UpdatedAt))
                != null)
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            Claim subjectClaim;
            if ((subjectClaim =
                    resourceOwner.Claims.FirstOrDefault(
                        c => c.Type == JwtConstants.OpenIdClaimTypes.Subject))
                != null)
            {
                resourceOwner.Claims.Remove(subjectClaim);
            }

            resourceOwner.Claims = resourceOwner.Claims.Add(
                new Claim(JwtConstants.OpenIdClaimTypes.Subject, request.Login),
                new Claim(JwtConstants.OpenIdClaimTypes.UpdatedAt, DateTime.UtcNow.ToString()));

            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.TheClaimsCannotBeUpdated);
            }

            return new OkResult();
        }

        [HttpPut("password")]
        [Authorize("manager")]
        public async Task<IActionResult> UpdatePassword(
            [FromBody] UpdateResourceOwnerPasswordRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("Parameter in request body not valid");
            }

            var resourceOwner =
                await _resourceOwnerRepository.Get(request.Login, cancellationToken).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.Password = request.Password;
            var result = await _resourceOwnerRepository.Update(resourceOwner, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.ThePasswordCannotBeUpdated);
            }

            return new OkResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add(
            [FromBody] AddResourceOwnerRequest addResourceOwnerRequest,
            CancellationToken cancellationToken)
        {
            if (addResourceOwnerRequest == null)
            {
                return BadRequest("Parameter in request body not valid");
            }

            if (await _addUserOperation.Execute(
                    new ResourceOwner
                    {
                        Id = addResourceOwnerRequest.Subject,
                        Password = addResourceOwnerRequest.Password
                    },
                    cancellationToken)
                .ConfigureAwait(false))
            {
                return NoContent();
            }

            return BadRequest(
                new ErrorResponse
                {
                    Error = ErrorCodes.UnhandledExceptionCode,
                    ErrorDescription = "a resource owner with same credentials already exists"
                });
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search(
            [FromBody] SearchResourceOwnersRequest searchResourceOwnersRequest,
            CancellationToken cancellationToken)
        {
            if (searchResourceOwnersRequest == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "Parameter in request body not valid",
                    HttpStatusCode.BadRequest);
            }

            var result = await _resourceOwnerRepository.Search(searchResourceOwnersRequest, cancellationToken)
                .ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }

        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse { Error = code, ErrorDescription = message };
            return new JsonResult(error) { StatusCode = (int)statusCode };
        }
    }
}
