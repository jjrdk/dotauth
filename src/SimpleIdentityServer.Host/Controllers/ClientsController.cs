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

namespace SimpleIdentityServer.Host.Controllers
{
    using Core.Errors;
    using Core.Exceptions;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;
    using Shared.Results;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    [Route(HostEnpoints.Clients)]
    public class ClientsController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository, IClientStore clientStore)
        {
            _clientRepository = clientRepository;
            _clientStore = clientStore;
        }

        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll()
        {
            //if (!await _representationManager.CheckRepresentationExistsAsync(this, GetClientsStoreName))
            //{
            //    return new ContentResult
            //    {
            //        StatusCode = 412
            //    };
            //}

            var result = await _clientStore.GetAllAsync().ConfigureAwait(false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName);
            return new OkObjectResult(result);
        }

        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search([FromBody] SearchClientsRequest request)
        {
            if (request == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var parameter = request.ToSearchClientParameter();
            var result = await _clientRepository.Search(parameter).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        [HttpGet("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            var result = await _clientStore.GetById(id).ConfigureAwait(false);
            if (result == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, ErrorDescriptions.TheClientDoesntExist, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            if (!await _clientRepository.Delete(id).ConfigureAwait(false))
            {
                return new BadRequestResult();
            }

            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientStoreName + id, false);
            //await _representationManager.AddOrUpdateRepresentationAsync(this, GetClientsStoreName, false);
            return new NoContentResult();
        }

        [HttpPut]
        [Authorize("manager")]
        public async Task<IActionResult> Put([FromBody] Client updateClientRequest)
        {
            if (updateClientRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            //var client = await _clientStore.GetById(updateClientRequest.ClientId).ConfigureAwait(false);
            try
            {
                var result = await _clientRepository.Update(updateClientRequest).ConfigureAwait(false);
                return result == null
                    ? (IActionResult)BadRequest(new ErrorResponse
                    {
                        Error = ErrorCodes.UnhandledExceptionCode,
                        ErrorDescription = ErrorDescriptions.RequestIsNotValid
                    })
                    : Ok(result);
            }
            catch (IdentityServerException e)
            {
                return BuildError(e.Code, e.Message, HttpStatusCode.BadRequest);
            }
            catch
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] Client client)
        {
            if (client == null)
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var existing = await _clientStore.GetById(client.ClientName).ConfigureAwait(false);
            if (existing != null)
            {
                return BadRequest();
            }

            var result = await _clientRepository.Insert(client).ConfigureAwait(false);

            return new OkObjectResult(result);
        }

        private IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse
            {
                Error = code,
                ErrorDescription = message
            };
            return StatusCode((int)statusCode, error);
        }
    }
}
