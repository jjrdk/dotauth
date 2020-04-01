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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Filters;

    /// <summary>
    /// Defines the client controller.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route(CoreConstants.EndPoints.Clients)]
    [ThrottleFilter]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly IScopeStore _scopeStore;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        /// <param name="clientRepository">The client repository.</param>
        /// <param name="scopeStore"></param>
        /// <param name="httpClient"></param>
        /// <param name="uriReader"></param>
        public ClientsController(
            IClientRepository clientRepository,
            IScopeStore scopeStore,
            HttpClient httpClient)
        {
            _clientRepository = clientRepository;
            _scopeStore = scopeStore;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "manager")]
        public async Task<ActionResult<Client[]>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _clientRepository.GetAll(cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Searches the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Search(
            [FromBody] SearchClientsRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BuildError(ErrorCodes.InvalidRequest, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            var result = await _clientRepository.Search(request, cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Gets the specified client.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequest, "identifier is missing", HttpStatusCode.BadRequest);
            }

            var result = await _clientRepository.GetById(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequest,
                    ErrorDescriptions.TheClientDoesntExist,
                    HttpStatusCode.NotFound);
            }

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequest, "identifier is missing", HttpStatusCode.BadRequest);
            }

            if (!await _clientRepository.Delete(id, cancellationToken).ConfigureAwait(false))
            {
                return new BadRequestObjectResult(
                    new ErrorDetails
                    {
                        Detail = "Could not delete client",
                        Status = HttpStatusCode.BadRequest,
                        Title = "Delete failed"
                    });
            }

            return new NoContentResult();
        }

        /// <summary>
        /// Puts the specified update client request.
        /// </summary>
        /// <param name="updateClientRequest">The update client request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Put([FromBody] Client updateClientRequest, CancellationToken cancellationToken)
        {
            if (updateClientRequest == null)
            {
                return BuildError(ErrorCodes.InvalidRequest, "no parameter in body request", HttpStatusCode.BadRequest);
            }

            try
            {
                var result = await _clientRepository.Update(updateClientRequest, cancellationToken)
                    .ConfigureAwait(false);
                return result == null
                    ? (IActionResult)BadRequest(
                        new ErrorDetails
                        {
                            Status = HttpStatusCode.BadRequest,
                            Title = ErrorCodes.UnhandledExceptionCode,
                            Detail = ErrorDescriptions.RequestIsNotValid
                        })
                    : Ok(result);
            }
            catch (SimpleAuthException e)
            {
                return BuildError(e.Code, e.Message, HttpStatusCode.BadRequest);
            }
            catch
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Adds the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = "manager")]
        public async Task<IActionResult> Add([FromBody] Client client, CancellationToken cancellationToken)
        {
            var factory = new ClientFactory(_httpClient, _scopeStore, JsonConvert.DeserializeObject<Uri[]>);
            client = await factory.Build(client, cancellationToken).ConfigureAwait(false);
            var result = await _clientRepository.Insert(client, cancellationToken).ConfigureAwait(false);

            return Ok(result);
        }

        private IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
            return StatusCode((int)statusCode, error);
        }
    }
}
