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
    using Errors;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using WebSite.User.Actions;

    [Route(CoreConstants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : Controller
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly AddUserOperation _addUserOperation;

        public ResourceOwnersController(
            IResourceOwnerRepository resourceOwnerRepository,
            IEnumerable<IAccountFilter> accountFilters,
            IEventPublisher eventPublisher)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _addUserOperation = new AddUserOperation(resourceOwnerRepository, accountFilters, eventPublisher);
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<IActionResult> Get()
        {
            var content = (await _resourceOwnerRepository.GetAllAsync().ConfigureAwait(false)).ToDtos();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners);
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the id parameter must be specified", HttpStatusCode.BadRequest);
            }

            var resourceOwner = await _resourceOwnerRepository.Get(id).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, id));
            }

            return Ok(resourceOwner.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the id parameter must be specified", HttpStatusCode.BadRequest);
            }

            if (!await _resourceOwnerRepository.Delete(id).ConfigureAwait(false))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = ErrorCodes.UnhandledExceptionCode,
                    ErrorDescription = ErrorDescriptions.TheResourceOwnerCannotBeRemoved
                });
            }

            return Ok();
        }

        [HttpPut("claims")]
        [Authorize("manager")]
        public async Task<IActionResult> UpdateClaims([FromBody] UpdateResourceOwnerClaimsRequest request)
        {
            if (request == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            //await _resourceOwnerActions.UpdateResourceOwnerClaims(request.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + request.Login, false);

            var resourceOwner = await _resourceOwnerRepository.Get(request.Login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidParameterCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.UpdateDateTime = DateTime.UtcNow;
            var claims = new List<Claim>();
            //var existingClaims = (await _claimRepository.GetAllAsync().ConfigureAwait(false)).ToArray();
            //if (existingClaims.Any() && request.Claims != null && request.Claims.Any())
            //{
            foreach (var claim in request.Claims)
            {
                //var cl = existingClaims.FirstOrDefault(c => c.Code == claim.Key);
                //if (cl == null)
                //{
                //    continue;
                //}

                claims.Add(new Claim(claim.Key, claim.Value));
            }
            //}

            resourceOwner.Claims = claims;
            Claim updatedClaim, subjectClaim;
            if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            if (((subjectClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject)) != null))
            {
                resourceOwner.Claims.Remove(subjectClaim);
            }

            resourceOwner.Claims.Add(new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, request.Login));
            resourceOwner.Claims.Add(new Claim(JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            var result = await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.TheClaimsCannotBeUpdated);
                //throw new SimpleAuthException(Core.Errors.ErrorCodes.InternalErrorCode, ErrorDescriptions.TheClaimsCannotBeUpdated);
            }

            return new OkResult();
        }

        [HttpPut("password")]
        [Authorize("manager")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateResourceOwnerPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Parameter in request body not valid");
            }

            var resourceOwner = await _resourceOwnerRepository.Get(request.Login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidParameterCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.Password = request.Password.ToSha256Hash();
            var result = await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.ThePasswordCannotBeUpdated);
            }

            return new OkResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] AddResourceOwnerRequest addResourceOwnerRequest)
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
                }).ConfigureAwait(false))
            {
                return NoContent();
            }
            return BadRequest(new ErrorResponse
            {
                Error = ErrorCodes.UnhandledExceptionCode,
                ErrorDescription = "a resource owner with same credentials already exists"
            });
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchResourceOwnersRequest searchResourceOwnersRequest)
        {
            if (searchResourceOwnersRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "Parameter in request body not valid", HttpStatusCode.BadRequest);
            }

            var result = await _resourceOwnerRepository.Search(searchResourceOwnersRequest.ToParameter()).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
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
