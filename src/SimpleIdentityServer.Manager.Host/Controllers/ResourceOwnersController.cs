#region copyright
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
#endregion

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners;
using SimpleIdentityServer.Manager.Host.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Host.Controllers
{
    [Route(Constants.EndPoints.ResourceOwners)]
    public class ResourceOwnersController : Controller
    {
        private readonly IResourceOwnerActions _resourceOwnerActions;

        public ResourceOwnersController(
            IResourceOwnerActions resourceOwnerActions)
        {
            _resourceOwnerActions = resourceOwnerActions;
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult> Get()
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, StoreNames.GetResourceOwners))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var content = (await _resourceOwnerActions.GetResourceOwners().ConfigureAwait(false)).ToDtos();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners);
            return new OkObjectResult(content);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the id parameter must be specified", HttpStatusCode.BadRequest);
            }
            
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, StoreNames.GetResourceOwner + id))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var content = (await _resourceOwnerActions.GetResourceOwner(id).ConfigureAwait(false)).ToDto();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + id);
            return new OkObjectResult(content);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "the id parameter must be specified", HttpStatusCode.BadRequest);
            }

            await _resourceOwnerActions.Delete(id).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners, false);
            return new NoContentResult();
        }

        [HttpPut("claims")]
        [Authorize("manager")]
        public async Task<ActionResult> UpdateClaims([FromBody] UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest)
        {
            if (updateResourceOwnerClaimsRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            await _resourceOwnerActions.UpdateResourceOwnerClaims(updateResourceOwnerClaimsRequest.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + updateResourceOwnerClaimsRequest.Login, false);
            return new OkResult();
        }

        [HttpPut("password")]
        [Authorize("manager")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest)
        {
            if (updateResourceOwnerPasswordRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            await _resourceOwnerActions.UpdateResourceOwnerPassword(updateResourceOwnerPasswordRequest.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwner + updateResourceOwnerPasswordRequest.Login, false);
            return new OkResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] AddResourceOwnerRequest addResourceOwnerRequest)
        {
            if (addResourceOwnerRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            await _resourceOwnerActions.Add(addResourceOwnerRequest.ToParameter()).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, StoreNames.GetResourceOwners, false);
            return new NoContentResult();
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchResourceOwnersRequest searchResourceOwnersRequest)
        {
            if (searchResourceOwnersRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var result = await _resourceOwnerActions.Search(searchResourceOwnersRequest.ToParameter()).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }
        
        private static JsonResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new SimpleIdentityServer.Common.Dtos.Responses.ErrorResponse
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
