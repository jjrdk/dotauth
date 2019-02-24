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
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Responses;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    /// <summary>
    /// Defines the client controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(CoreConstants.EndPoints.Clients)]
    public class ClientsController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly IClientRepository _clientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        /// <param name="clientRepository">The client repository.</param>
        /// <param name="clientStore">The client store.</param>
        public ClientsController(IClientRepository clientRepository, IClientStore clientStore)
        {
            _clientRepository = clientRepository;
            _clientStore = clientStore;
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize("manager")]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _clientStore.GetAll(cancellationToken).ConfigureAwait(false);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Searches the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost(".search")]
        [Authorize("manager")]
        public async Task<IActionResult> Search(
            [FromBody] SearchClientsRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
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
        [Authorize("manager")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            var result = await _clientStore.GetById(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
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
        [Authorize("manager")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BuildError(ErrorCodes.InvalidRequestCode, "identifier is missing", HttpStatusCode.BadRequest);
            }

            if (!await _clientRepository.Delete(id, cancellationToken).ConfigureAwait(false))
            {
                return new BadRequestResult();
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
        [Authorize("manager")]
        public async Task<IActionResult> Put([FromBody] Client updateClientRequest, CancellationToken cancellationToken)
        {
            if (updateClientRequest == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            try
            {
                var result = await _clientRepository.Update(updateClientRequest, cancellationToken)
                    .ConfigureAwait(false);
                return result == null
                    ? (IActionResult) BadRequest(
                        new ErrorResponse
                        {
                            Error = ErrorCodes.UnhandledExceptionCode,
                            ErrorDescription = ErrorDescriptions.RequestIsNotValid
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
        [Authorize("manager")]
        public async Task<IActionResult> Add([FromBody] Client client, CancellationToken cancellationToken)
        {
            if (client == null)
            {
                return BuildError(
                    ErrorCodes.InvalidRequestCode,
                    "no parameter in body request",
                    HttpStatusCode.BadRequest);
            }

            var existing = await _clientStore.GetById(client.ClientName, cancellationToken).ConfigureAwait(false);
            if (existing != null)
            {
                return BadRequest();
            }

            var result = await _clientRepository.Insert(client, cancellationToken).ConfigureAwait(false);

            return new OkObjectResult(result);
        }

        private IActionResult BuildError(string code, string message, HttpStatusCode statusCode)
        {
            var error = new ErrorResponse {Error = code, ErrorDescription = message};
            return StatusCode((int) statusCode, error);
        }
    }
}
