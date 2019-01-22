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
    using System.Threading.Tasks;
    using Errors;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;

    [Route(CoreConstants.EndPoints.Scopes)]
    public class ScopesController : Controller
    {
        private readonly IScopeRepository _scopeRepository;

        public ScopesController(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
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
            var result = await _scopeRepository.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result.ToDto());
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<IActionResult> GetAll()
        {
            var result = (await _scopeRepository.GetAll().ConfigureAwait(false)).ToDtos();
            return new OkObjectResult(result);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id)
        {
            var result = (await _scopeRepository.Get(id).ConfigureAwait(false)).ToDto();
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

            var scope = await _scopeRepository.Get(id).ConfigureAwait(false);
            if (scope == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = ErrorCodes.InvalidRequestCode,
                    ErrorDescription = string.Format(ErrorDescriptions.TheScopeDoesntExist, id)
                });
            }
            var deleted = await _scopeRepository.Delete(scope).ConfigureAwait(false);

            return deleted
                ? NoContent()
                : (IActionResult)BadRequest(new ErrorResponse
                {
                    Error = ErrorCodes.InvalidRequestCode,
                    ErrorDescription = string.Format(ErrorDescriptions.TheScopeDoesntExist, id)
                });
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] ScopeResponse request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!await _scopeRepository.Insert(request.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

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

            if (!await _scopeRepository.Update(request.ToParameter()).ConfigureAwait(false))
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new NoContentResult();
        }
    }
}
