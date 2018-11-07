// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleIdentityServer.Manager.Common.Requests;
    using System.Net;
    using System.Threading.Tasks;

    using Core.Exceptions;
    using SimpleIdentityServer.Core.Helpers;
    using SimpleIdentityServer.Core.WebSite.User.Actions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Responses;
    using SimpleIdentityServer.Core.Errors;
    using SimpleIdentityServer.Host.Extensions;
    using ErrorCodes = SimpleIdentityServer.Core.Errors.ErrorCodes;

    [Route(Constants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : Controller
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAddUserOperation _addUserOperation;

        public ResourceOwnersController(
            IResourceOwnerRepository resourceOwnerRepository,
            IClaimRepository claimRepository,
            IAddUserOperation addUserOperation)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _addUserOperation = addUserOperation;
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
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, id));
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
                return StatusCode((int)HttpStatusCode.BadRequest);
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
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.UpdateDateTime = DateTime.UtcNow;
            var claims = new List<Claim>();
            var existingClaims = (await _claimRepository.GetAllAsync().ConfigureAwait(false)).ToArray();
            if (existingClaims != null && existingClaims.Any() && request.Claims != null && request.Claims.Any())
            {
                foreach (var claim in request.Claims)
                {
                    var cl = existingClaims.FirstOrDefault(c => c.Code == claim.Key);
                    if (cl == null)
                    {
                        continue;
                    }

                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            resourceOwner.Claims = claims;
            Claim updatedClaim, subjectClaim;
            if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == SimpleIdentityServer.Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            if (((subjectClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == SimpleIdentityServer.Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject)) != null))
            {
                resourceOwner.Claims.Remove(subjectClaim);
            }

            resourceOwner.Claims.Add(new Claim(SimpleIdentityServer.Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.Subject, request.Login));
            resourceOwner.Claims.Add(new Claim(SimpleIdentityServer.Core.Jwt.JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            var result = await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.TheClaimsCannotBeUpdated);
                //throw new IdentityServerManagerException(Core.Errors.ErrorCodes.InternalErrorCode, ErrorDescriptions.TheClaimsCannotBeUpdated);
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

            //await _resourceOwnerActions.UpdateResourceOwnerPassword(updateResourceOwnerPasswordRequest.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + updateResourceOwnerPasswordRequest.Login, false);
            var resourceOwner = await _resourceOwnerRepository.Get(request.Login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.Password = request.Password.ToSha256Hash();
            var result = await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            if (!result)
            {
                return BadRequest(ErrorDescriptions.ThePasswordCannotBeUpdated);
                //throw new IdentityServerManagerException(Core.Errors.ErrorCodes.InternalErrorCode, ErrorDescriptions.ThePasswordCannotBeUpdated);
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
                NoContent();
            }
            //await _resourceOwnerActions.Add(addResourceOwnerRequest.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners, false);
            return BadRequest();
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
