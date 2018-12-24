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

namespace SimpleIdentityServer.Host.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Core;
    using Core.Api.Scopes;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    [Route(CoreConstants.EndPoints.Scopes)]
    public class ScopesController : Controller
    {
        private readonly IScopeActions _scopeActions;

        public ScopesController(IScopeActions scopeActions)
        {
            _scopeActions = scopeActions;
        }
        
        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchScopesRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var parameter = request.ToSearchScopesParameter();
            var result = await _scopeActions.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<IActionResult> GetAll()
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, ScopesStoreName))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = (await _scopeActions.GetScopes().ConfigureAwait(false)).ToDtos();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName);
            return new OkObjectResult(result);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id)
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, ScopeStoreName + id))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = (await _scopeActions.GetScope(id).ConfigureAwait(false)).ToDto();
            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + id);
            return new OkObjectResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            await _scopeActions.DeleteScope(id).ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName, false);
            return new NoContentResult();
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] ScopeResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!await _scopeActions.AddScope(request.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopesStoreName, false);
            return new NoContentResult();
        }
        
        [HttpPut]
        [Authorize("manager")]
        public async Task<IActionResult> Update([FromBody] ScopeResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!await _scopeActions.UpdateScope(request.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, ScopeStoreName + request.Name, false);
            return new NoContentResult();
        }
    }
}
